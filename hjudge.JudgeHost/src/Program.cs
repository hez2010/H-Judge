using hjudge.Shared.MessageQueue;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace hjudge.JudgeHost
{
    class Program
    {
        public static Task Main(string[] args) => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;
                    services.AddHostedService<JudgeQueue>()
                        .Configure<JudgeHostConfig>(options =>
                        {
                            options.FileHost = config["FileHost"];
                            options.DataCacheDirectory = config["DataCacheDirectory"];
                            options.MessageQueue = new MessageQueueOptions
                            {
                                HostName = config["MessageQueue:HostName"],
                                Password = config["MessageQueue:Password"],
                                Port = int.Parse(config["MessageQueue:Port"]),
                                UserName = config["MessageQueue:UserName"],
                                VirtualHost = config["MessageQueue:VirtualHost"]
                            };

                            var messageQueueOptions = config.GetSection("MessageQueue");
                            var producersConfig = messageQueueOptions.GetSection("Producers").GetChildren();
                            var consumersConfig = messageQueueOptions.GetSection("Consumers").GetChildren();

                            var producers = new List<MessageQueueFactory.ProducerOptions>();
                            foreach (var i in producersConfig)
                            {
                                producers.Add(new MessageQueueFactory.ProducerOptions
                                {
                                    AutoDelete = bool.Parse(i["AutoDelete"]),
                                    Durable = bool.Parse(i["Durable"]),
                                    Exchange = i["Exchange"],
                                    Exclusive = bool.Parse(i["Exclusive"]),
                                    Queue = i["Queue"],
                                    RoutingKey = i["RoutingKey"]
                                });
                            }
                            options.MessageQueue.Producers = producers.ToArray();
                            var consumers = new List<MessageQueueFactory.ConsumerOptions>();
                            foreach (var i in consumersConfig)
                            {
                                consumers.Add(new MessageQueueFactory.ConsumerOptions
                                {
                                    AutoAck = bool.Parse(i["AutoAck"]),
                                    Durable = bool.Parse(i["Durable"]),
                                    Exchange = i["Exchange"],
                                    Exclusive = bool.Parse(i["Exclusive"]),
                                    Queue = i["Queue"],
                                    RoutingKey = i["RoutingKey"]
                                });
                            }
                            options.MessageQueue.Consumers = consumers.ToArray();

                            options.DataCacheDirectory = Path.Combine(
                                Path.GetTempPath(),
                                config["DataCacheDirectory"],
                                Guid.NewGuid().ToString().Replace("-", string.Empty));
                        });
                });
    }
}
