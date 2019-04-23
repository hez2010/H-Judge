using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IRabbitService
    {
        void BasicPublish(string exchangeName);
    }
    public class RabbitService : IDisposable, IRabbitService
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly RabbitOptions config;
        public RabbitService(IOptions<RabbitOptions> options)
        {
            config = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = config.HostName,
                Port = config.Port,
                UserName = config.UserName,
                Password = config.Password,
                VirtualHost = config.VirtualHost
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            if (config.ExchangeOptions != null)
            {
                channel.ExchangeDeclare(
                    exchange: config.ExchangeOptions.Name,
                    durable: config.ExchangeOptions.Durable,
                    autoDelete: config.ExchangeOptions.AutoDelete,
                    arguments: config.ExchangeOptions.Arguments,
                    type: config.ExchangeOptions.Type);
            }

            if (config.QueueOptions != null)
            {
                channel.QueueDeclare(
                    queue: config.QueueOptions.Name,
                    durable: config.QueueOptions.Durable,
                    exclusive: config.QueueOptions.Exclusive,
                    autoDelete: config.QueueOptions.AutoDelete,
                    arguments: config.QueueOptions.Arguments);
            }

            if (config.ExchangeOptions != null && config.QueueOptions != null)
            {
                channel.QueueBind(config.QueueOptions.Name, config.ExchangeOptions.Name, config.RoutingKey);
            }
        }

        public void Dispose()
        {
            channel.Close();
            connection.Close();
            channel.Dispose();
            connection.Dispose();
        }
    }

    public class RabbitOptions
    {
        public class ModelOptions
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = ExchangeType.Direct;
            public bool Durable { get; set; } = true;
            public bool Exclusive { get; set; } = false;
            public bool AutoDelete { get; set; } = false;
            public IDictionary<string, object>? Arguments { get; set; }
        }
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; } = 5672;
        public ModelOptions? ExchangeOptions { get; set; }
        public ModelOptions? QueueOptions { get; set; }
        public string RoutingKey { get; set; } = string.Empty;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
    }
}
