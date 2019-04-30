using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace hjudgeJudgeHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var semaphore = new SemaphoreSlim(0, 1);
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "JudgeReport", durable: true, exclusive: false, autoDelete: false);
            channel.ExchangeDeclare("JudgeReport_Exchange", "direct", true, false);

            channel.QueueBind("JudgeReport", "JudgeReport_Exchange", "H::Judge_JudgeReport");
            var props = channel.CreateBasicProperties();
            props.ContentType = "plain/text";
            channel.BasicPublish("JudgeReport_Exchange", "H::Judge_JudgeReport", props, Encoding.UTF8.GetBytes("test"));
            semaphore.Wait();
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);
            var judgeInfo = JsonConvert.DeserializeObject<JudgeInfo>(message);

        }
    }
}
