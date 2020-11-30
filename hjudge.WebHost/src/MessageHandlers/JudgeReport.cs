using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hjudge.Core;
using hjudge.Shared.Judge;
using hjudge.Shared.Utils;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Hubs;
using hjudge.WebHost.Hubs.Clients;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace hjudge.WebHost.MessageHandlers
{
    public class JudgeReport
    {
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static readonly ConcurrentQueue<JudgeReportInfo> queue = new ConcurrentQueue<JudgeReportInfo>();
        public static Task JudgeReport_Received(object sender, BasicDeliverEventArgs args)
        {
            if (!(sender is AsyncEventingBasicConsumer consumer)) return Task.CompletedTask;

            try
            {
                queue.Enqueue(args.Body.Span.DeserializeJson<JudgeReportInfo>(false));
            }
            catch
            {
                consumer.Model.BasicNack(args.DeliveryTag, false, !args.Redelivered);
                if (args.Redelivered)
                {
                    Console.WriteLine($@"{DateTime.Now}: Message fetching failed, tag: {args.DeliveryTag}");
                }
                return Task.CompletedTask;
            }
            try
            {
                semaphore.Release();
            }
            catch { /* ignored */ }
            consumer.Model.BasicAck(args.DeliveryTag, false);
            return Task.CompletedTask;
        }

        public static async Task QueueExecutor(CancellationToken token)
        {
            var random = new Random();
            while (!token.IsCancellationRequested)
            {
                await semaphore.WaitAsync(token);
                while (queue.TryDequeue(out var info))
                {
                    using var scope = Program.RootServiceProvider?.CreateScope();
                    try
                    {
                        var judgeService = scope?.ServiceProvider.GetService<IJudgeService>();
                        if (judgeService is null) throw new InvalidOperationException("IJudgeService was not registed into service collection.");

                        var dbContext = scope?.ServiceProvider.GetService<WebHostDbContext>();
                        if (dbContext is null) throw new InvalidOperationException("WebHostDbContext was not registed into service collection.");

                        if (info.Type == JudgeReportInfo.ReportType.PostJudge)
                        {
                            var userManager = scope?.ServiceProvider.GetService<UserManager<UserInfo>>();
                            if (userManager is null) throw new InvalidOperationException("UserManager<UserInfo> was not registed into service collection.");

                            var resultType = JudgeService.ComputeJudgeResultType(info.JudgeResult);
                            
                            var judge = await judgeService.GetJudgeAsync(info.JudgeId);
                            if (judge != null && judge.JudgeCount <= 1 && (int)resultType >= (int)ResultCode.Accepted)
                            {
                                if (resultType == ResultCode.Accepted)
                                {
                                    if (judge.ContestId != null)
                                    {
                                        var problemConfig = await dbContext.ContestProblemConfig
                                            .Where(i => i.ContestId == judge.ContestId &&
                                                        i.ProblemId == judge.ProblemId)
                                            .FirstOrDefaultAsync(token);
                                        problemConfig.AcceptCount++;
                                        dbContext.ContestProblemConfig.Update(problemConfig);
                                    }
                                    else
                                    {
                                        var problem = await dbContext.Problem
                                            .Where(i => i.Id == judge.ProblemId)
                                            .FirstOrDefaultAsync(token);
                                        problem.AcceptCount++;
                                        dbContext.Problem.Update(problem);
                                    }
                                }

                                var (coins, experience, accept) = resultType switch
                                {
                                    ResultCode.Accepted => (random.Next(30, 80), random.Next(50, 100), 1),
                                    ResultCode.Presentation_Error => (random.Next(10, 30), random.Next(30, 50), 0),
                                    _ => (0, random.Next(10, 30), 0)
                                };

                                var userInfo = await userManager.FindByIdAsync(judge.UserId);

                                userInfo.Coins += coins;
                                userInfo.Experience += experience;
                                userInfo.AcceptedCount += accept;

                                await userManager.UpdateAsync(userInfo);
                            }

                            await dbContext.SaveChangesAsync();
                        }

                        await judgeService.UpdateJudgeResultAsync(info.JudgeId, info.Type, info.JudgeResult);

                        var judgeHub = Program.RootServiceProvider?.GetService<IHubContext<JudgeHub, IJudgeHub>>();
                        if (judgeHub is null) throw new InvalidOperationException("IHubContext<JudgeHub, IJudgeHub> was not registed into service collection.");
                        await judgeHub.Clients.Group($"result_{info.JudgeId}").JudgeCompleteSignalReceived(info.JudgeId);

                    }
                    catch (Exception ex)
                    {
                        var logger = scope?.ServiceProvider.GetService<ILogger<JudgeReport>>();
                        if (logger is null) throw new NullReferenceException();
                        logger.LogError(ex, "Judge report update error.");
                    }
                }
            }
        }
    }
}
