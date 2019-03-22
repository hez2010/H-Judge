using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public class RabbitService
    {
        public RabbitService(IOptions<RabbitOptions> options)
        {
            var config = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = config.HostName,
                Port = config.Port
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: config.Queue,
                durable: config.Durable,
                exclusive: config.Exclusive,
                autoDelete: config.AutoDelete,
                arguments: config.Arguments);
        }
    }

    public class RabbitOptions
    {
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Queue { get; set; } = string.Empty;
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
        public IDictionary<string, object>? Arguments { get; set; }
    }
}
