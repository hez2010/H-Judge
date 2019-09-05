using hjudge.Shared.Utils;
using hjudge.Shared.MessageQueue;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using hjudge.Shared.Judge;
using System.Collections.Generic;
using Grpc.Core;
using RabbitMQ.Client;

namespace hjudge.JudgeHost
{
    class Program
    {

        public static MessageQueueFactory? JudgeMessageQueueFactory;
        public static Channel? FileHostChannel;
        private static readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
#if DEBUG
            var config = File.ReadAllText("appsettings.Development.json", Encoding.UTF8).DeserializeJson<JudgeHostConfig>(false);
#else
            var config = File.ReadAllText("appsettings.json", Encoding.UTF8).DeserializeJson<JudgeHostConfig>(false);
#endif
            if (config.ConcurrentJudgeTask <= 0) config.ConcurrentJudgeTask = Environment.ProcessorCount;
            JudgeQueue.Semaphore = new SemaphoreSlim(0, config.ConcurrentJudgeTask);

            FileHostChannel = new Channel(config.FileHost, ChannelCredentials.Insecure);
            var options = config.MessageQueue;
            if (options != null)
            {
                JudgeMessageQueueFactory = new MessageQueueFactory(new MessageQueueFactory.HostOptions
                {
                    HostName = options.HostName,
                    Password = options.Password,
                    Port = options.Port,
                    UserName = options.UserName,
                    VirtualHost = options.VirtualHost
                });

                if (options.Producers != null)
                {
                    foreach (var i in options.Producers)
                    {
                        JudgeMessageQueueFactory.CreateProducer(i);
                    }
                }

                if (options.Consumers != null)
                {
                    foreach (var i in options.Consumers)
                    {
                        i.OnReceived = i.Queue switch
                        {
                            "JudgeQueue" => new AsyncEventHandler<BasicDeliverEventArgs>(JudgeRequest_Received),
                            _ => null
                        };

                        JudgeMessageQueueFactory.CreateConsumer(i);
                    }
                }
            }
            var token = tokenSource.Token;

            Console.CancelKeyPress += async (sender, e) =>
            {
                await FileHostChannel.ShutdownAsync();
                tokenSource.Cancel();
                e.Cancel = true;
            };

            var tasks = new List<Task>();
            for (var i = 0; i < config.ConcurrentJudgeTask; i++) 
            tasks.Add(JudgeQueue.JudgeQueueExecutor(
                Path.Combine(
                    Path.GetTempPath(),
                    config.DataCacheDirectory,
                    Guid.NewGuid().ToString().Replace("-", "_")),
                token));

            await Task.WhenAll(tasks);

            JudgeMessageQueueFactory?.Dispose();
        }

        private static async Task JudgeRequest_Received(object sender, BasicDeliverEventArgs args)
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
                        Console.WriteLine($"{DateTime.Now}: Message fetching failed, tag: {args.DeliveryTag}");
                    }
                    return;
                }

                await JudgeQueue.QueueJudgeAsync(info);

                consumer.Model.BasicAck(args.DeliveryTag, false);
            }
        }
    }
}
