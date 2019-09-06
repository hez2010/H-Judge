using hjudge.Shared.Judge;
using hjudge.WebHost.Services;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using hjudge.Shared.Utils;
using System;
using System.Threading.Tasks;
using hjudge.WebHost.Extensions;
using hjudge.WebHost.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading;
using hjudge.Core;
using hjudge.WebHost.Data.Identity;

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
                queue.Enqueue(args.Body.DeserializeJson<JudgeReportInfo>(false));
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
            while (!token.IsCancellationRequested)
            {
                await semaphore.WaitAsync(token);
                if (queue.TryDequeue(out var info))
                {
                    var judgeService = ServiceProviderExtensions.ServiceProvider?.GetService<IJudgeService>();
                    if (judgeService == null) throw new InvalidOperationException("IJudgeService was not registed into service collection.");

                    await judgeService.UpdateJudgeResultAsync(info.JudgeId, info.Type, info.JudgeResult);

                    var judgeHub = Program.RootServiceProvider?.GetService<IHubContext<JudgeHub, IJudgeHub>>();
                    if (judgeHub == null) throw new InvalidOperationException("IHubContext<JudgeHub, IJudgeHub> was not registed into service collection.");
                    await judgeHub.Clients.Group($"result_{info.JudgeId}").JudgeCompleteSignalReceived(info.JudgeId);

                    var userManager = ServiceProviderExtensions.ServiceProvider?.GetService<CachedUserManager<UserInfo>>();
                    if (userManager == null) throw new InvalidOperationException("CachedUserManager<UserInfo> was not registed into service collection.");
                    var judge = await judgeService.GetJudgeAsync(info.JudgeId);
                    if (judge != null && judge.JudgeCount <= 1 && judge.ResultType == (int)ResultCode.Accepted)
                    {
                        var user = await userManager.FindByIdAsync(judge.UserId);
                        user.AcceptedCount++;
                        await userManager.UpdateAsync(user);
                    }
                }
            }
        }
    }
}
