using hjudgeCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hjudgeJudgeHost
{
    class Program
    {
        class MessageQueueOptions
        {
            public string HostName { get; set; } = string.Empty;
            public string VirtualHost { get; set; } = "/";
            public int Port { get; set; } = 5672;
            public string UserName { get; set; } = "guest";
            public string Password { get; set; } = "guest";
            public MessageQueueFactory.ProducerOptions[]? Producers { get; set; }
            public MessageQueueFactory.ConsumerOptions[]? Consumers { get; set; }
        }
        class JudgeHostConfig
        {
            public MessageQueueOptions? MessageQueue { get; set; }
        }

        public static MessageQueueFactory JudgeMessageQueueFactory;
        private static readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            //FIX ME: json parse err.
#if DEBUG
            var config = File.ReadAllText("appsettings.Development.json", Encoding.UTF8).DeserializeJson<JudgeHostConfig>();
#endif
#if RELEASE
            var config = File.ReadAllText("appsettings.json", Encoding.UTF8).DeserializeJson<JudgeHostConfig>();
#endif
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

            Console.CancelKeyPress += (sender, e) =>
            {
                tokenSource.Cancel();
                e.Cancel = true;
            };

            await JudgeQueue.JudgeQueueExecuter(token);
        }

        private static async Task JudgeRequest_Received(object sender, BasicDeliverEventArgs args)
        {
            if (sender is AsyncEventingBasicConsumer consumer)
            {
                JudgeInfo info;

                try
                {
                    info = args.Body.DeserializeJson<JudgeInfo>();
                }
                catch
                {
                    consumer.Model.BasicAck(args.DeliveryTag, false);
                    return;
                }

                await JudgeQueue.QueueJudgeAsync(info);

                consumer.Model.BasicAck(args.DeliveryTag, false);
            }
        }
    }
}
