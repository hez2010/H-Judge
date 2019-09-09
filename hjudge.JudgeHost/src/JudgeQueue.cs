using Grpc.Core;
using hjudge.Core;
using hjudge.Shared.Judge;
using hjudge.Shared.MessageQueue;
using hjudge.Shared.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace hjudge.JudgeHost
{
    class JudgeQueue : BackgroundService
    {
        private readonly ILogger<JudgeQueue> logger;
        private readonly JudgeHostConfig options;
        private readonly MessageQueueFactory queueFactory;
        private readonly ConcurrentPriorityQueue<((ulong DeliveryTag, AsyncEventingBasicConsumer Consumer) Sender, JudgeInfo JudgeInfo)> pools = new ConcurrentPriorityQueue<((ulong DeliveryTag, AsyncEventingBasicConsumer Consumer) Sender, JudgeInfo JudgeInfo)>();
        private readonly ConcurrentDictionary<string, long> fileCache = new ConcurrentDictionary<string, long>();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly Channel fileHostChannel;

        public JudgeQueue(
            ILogger<JudgeQueue> logger,
            IOptions<JudgeHostConfig> options)
        {
            this.logger = logger;
            this.options = options.Value;

            if (this.options.MessageQueue == null) throw new InvalidOperationException("Message queue config cannot be null.");

            queueFactory = new MessageQueueFactory(new MessageQueueFactory.HostOptions
            {
                HostName = this.options.MessageQueue.HostName,
                Password = this.options.MessageQueue.Password,
                Port = this.options.MessageQueue.Port,
                UserName = this.options.MessageQueue.UserName,
                VirtualHost = this.options.MessageQueue.VirtualHost
            });

            if (this.options.MessageQueue.Producers != null)
            {
                foreach (var i in this.options.MessageQueue.Producers)
                {
                    queueFactory.CreateProducer(i);
                }
            }

            if (this.options.MessageQueue.Consumers != null)
            {
                foreach (var i in this.options.MessageQueue.Consumers)
                {
                    i.OnReceived = i.Queue switch
                    {
                        "JudgeQueue" => new AsyncEventHandler<BasicDeliverEventArgs>(JudgeRequest_Received),
                        _ => null
                    };

                    queueFactory.CreateConsumer(i);
                }
            }

            fileHostChannel = new Channel(this.options.FileHost, ChannelCredentials.Insecure);
        }

        private Task JudgeRequest_Received(object sender, BasicDeliverEventArgs args)
        {
            if (sender is AsyncEventingBasicConsumer consumer)
            {
                JudgeInfo info;

                try
                {
                    info = args.Body.DeserializeJson<JudgeInfo>(false);
                }
                catch
                {
                    consumer.Model.BasicNack(args.DeliveryTag, false, !args.Redelivered);
                    if (args.Redelivered)
                    {
                        logger.LogInformation($"Message fetching failed, tag: {args.DeliveryTag}");
                    }
                    return Task.CompletedTask;
                }

                logger.LogInformation($"Received judge request #{info.JudgeId}");
                pools.Enqueue(((args.DeliveryTag, consumer), info), info.Priority);
                try
                {
                    semaphore.Release();
                }
                catch { /* ignored */ }
            }
            return Task.CompletedTask;
        }

        private Task ReportJudgeResultAsync(JudgeReportInfo result)
        {
            var (channel, options) = queueFactory.GetProducer("JudgeReport");
            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            channel.BasicPublish(options.Exchange, options.RoutingKey, false, props, result.SerializeJson(false));
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(options.DataCacheDirectory)) Directory.CreateDirectory(options.DataCacheDirectory);
            if (semaphore == null) throw new InvalidOperationException("JudgeQueue.Semaphore cannot be null.");
            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                while (!stoppingToken.IsCancellationRequested)
                {
                    await semaphore.WaitAsync(stoppingToken);
                    while (pools.TryDequeue(out var info))
                    {
                        try
                        {
                            var judgeInfo = info.JudgeInfo;
                            logger.LogInformation($"Started judge #{judgeInfo.JudgeId}");
                            await ReportJudgeResultAsync(new JudgeReportInfo
                            {
                                JudgeId = judgeInfo.JudgeId,
                                JudgeResult = null,
                                Type = JudgeReportInfo.ReportType.PreJudge
                            });

                            var judge = new JudgeMain();
                            if (judgeInfo.JudgeOptions == null || judgeInfo.BuildOptions == null)
                            {
                                await ReportJudgeResultAsync(new JudgeReportInfo
                                {
                                    JudgeId = judgeInfo.JudgeId,
                                    JudgeResult = new JudgeResult { JudgePoints = null },
                                    Type = JudgeReportInfo.ReportType.PostJudge
                                });
                                continue;
                            }

                            var workingDir = Path.Combine(Path.GetTempPath(), "hjudgeTest", judgeInfo.JudgeOptions.GuidStr);

                            var filesRequired =
                                (await VarsProcessor.FillinWorkingDirAndGetRequiredFiles(judgeInfo, workingDir)).Distinct();

                            logger.LogInformation($"Started downloading files for #{judgeInfo.JudgeId}");
                            var fileService = new Files.FilesClient(fileHostChannel);

                            var listRequest = new ListExactRequest();
                            listRequest.FileNames.AddRange(filesRequired);

                            var fileInfos =
                                (await fileService.ListExactFilesAsync(listRequest))
                                .FileInfos.ToDictionary(i => i.FileName, i => i.LastModified);

                            var request = new DownloadRequest();

                            foreach (var i in filesRequired)
                            {
                                var cache = fileCache.Where(j => j.Key == i).ToList();
                                if (!fileInfos.ContainsKey(i)) continue;
                                if (!cache.Any())
                                {
                                    request.FileNames.Add(i);
                                    fileCache[i] = fileInfos[i];
                                }
                                else if (cache.FirstOrDefault().Value != fileInfos[i])
                                {
                                    request.FileNames.Add(i);
                                    fileCache[i] = fileInfos[i];
                                }
                            }

                            var filesResponse = fileService.DownloadFiles(request, null, null, stoppingToken);

                            while (await filesResponse.ResponseStream.MoveNext(stoppingToken))
                            {
                                foreach (var i in filesResponse.ResponseStream.Current.Result)
                                {
                                    var fileName = Path.Combine(options.DataCacheDirectory, JudgeMain.EscapeFileName(i.FileName));
                                    FileMode mode;
                                    if (File.Exists(fileName)) mode = FileMode.Truncate;
                                    else mode = FileMode.CreateNew;
                                    try
                                    {
                                        await using var fs = new FileStream(fileName, mode, FileAccess.ReadWrite,
                                            FileShare.None);
                                        i.Content.WriteTo(fs);
                                        await fs.FlushAsync(stoppingToken);
                                        fs.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                            }

                            logger.LogInformation($"Finished downloading files for #{judgeInfo.JudgeId}");

                            var result = new JudgeResult { JudgePoints = null };
                            try
                            {
                                result = await judge.JudgeAsync(judgeInfo.BuildOptions, judgeInfo.JudgeOptions, workingDir,
                                    options.DataCacheDirectory);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, $"Judge Error for #{judgeInfo.JudgeId}");
                            }

                            logger.LogInformation($"Finished judge #{judgeInfo.JudgeId}");
                            await ReportJudgeResultAsync(new JudgeReportInfo
                            {
                                JudgeId = judgeInfo.JudgeId,
                                JudgeResult = result,
                                Type = JudgeReportInfo.ReportType.PostJudge
                            });
                            logger.LogInformation($"Reported judge #{judgeInfo.JudgeId}");
                        }
                        finally
                        {
                            info.Sender.Consumer.Model.BasicAck(info.Sender.DeliveryTag, false);
                        }
                    }
                }
            }
            finally
            {
                Directory.Delete(options.DataCacheDirectory, true);
                await fileHostChannel.ShutdownAsync();
            }
        }
    }
}
