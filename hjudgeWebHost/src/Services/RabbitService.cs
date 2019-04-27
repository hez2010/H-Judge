using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IRabbitService
    {
        void PublishMessage(byte[] message);
    }
    public class RabbitService : IRabbitService, IHostedService
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
        }

        public void PublishMessage(byte[] message)
        {
            if (config.Role == 1) throw new InvalidOperationException("Consumer cannot publish messages.");
            if (config.ExchangeOptions == null) throw new ArgumentNullException("Exchange options cannot be null.");
            channel.BasicPublish(config.ExchangeOptions.Name, config.RoutingKey, body: message);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (config.Role == 0)
            {
                if (config.QueueOptions != null)
                {
                    channel.QueueDeclare(
                        queue: config.QueueOptions.Name,
                        durable: config.QueueOptions.Durable,
                        exclusive: config.QueueOptions.Exclusive,
                        autoDelete: config.QueueOptions.AutoDelete,
                        arguments: config.QueueOptions.Arguments);
                }

                if (config.ExchangeOptions != null)
                {
                    channel.ExchangeDeclare(
                        exchange: config.ExchangeOptions.Name,
                        durable: config.ExchangeOptions.Durable,
                        autoDelete: config.ExchangeOptions.AutoDelete,
                        arguments: config.ExchangeOptions.Arguments,
                        type: config.ExchangeOptions.Type);
                }

                if (config.ExchangeOptions != null && config.QueueOptions != null)
                {
                    channel.QueueBind(config.QueueOptions.Name, config.ExchangeOptions.Name, config.RoutingKey);
                }
            }
            else
            {
                if (config.ConsumerOptions != null)
                {
                    var consumer = new AsyncEventingBasicConsumer(channel);

                    if (config.ConsumerOptions.OnReceived != null) consumer.Received += config.ConsumerOptions.OnReceived;
                    if (config.ConsumerOptions.OnRegistered != null) consumer.Registered += config.ConsumerOptions.OnRegistered;
                    if (config.ConsumerOptions.OnUnregistered != null) consumer.Unregistered += config.ConsumerOptions.OnUnregistered;
                    if (config.ConsumerOptions.OnShutdown != null) consumer.Shutdown += config.ConsumerOptions.OnShutdown;
                    if (config.ConsumerOptions.OnCancelled != null) consumer.ConsumerCancelled += config.ConsumerOptions.OnCancelled;

                    channel.BasicConsume(consumer, config.ConsumerOptions.QueueName, config.ConsumerOptions.AutoAck, config.ConsumerOptions.Tag, false, config.ConsumerOptions.Exclusive, config.ConsumerOptions.Arguments);
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            channel.Close();
            connection.Close();
            channel.Dispose();
            connection.Dispose();
            return Task.CompletedTask;
        }
    }

    public class ProducerOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = ExchangeType.Direct;
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
        public IDictionary<string, object>? Arguments { get; set; }
    }
    public class ConsumerOptions
    {
        public string QueueName { get; set; } = string.Empty;
        public AsyncEventHandler<BasicDeliverEventArgs>? OnReceived { get; set; }
        public AsyncEventHandler<ConsumerEventArgs>? OnRegistered { get; set; }
        public AsyncEventHandler<ConsumerEventArgs>? OnCancelled { get; set; }
        public AsyncEventHandler<ShutdownEventArgs>? OnShutdown { get; set; }
        public AsyncEventHandler<ConsumerEventArgs>? OnUnregistered { get; set; }
        public bool Exclusive { get; set; } = false;
        public bool AutoAck { get; set; } = true;
        public string? Tag { get; set; }
        public IDictionary<string, object>? Arguments { get; set; }
    }
    public class RabbitOptions
    {
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; } = 5672;
        /// <summary>
        /// 0 - Producer, 1 - Consumer
        /// </summary>
        public int Role { get; set; }
        public ProducerOptions? ExchangeOptions { get; set; }
        public ProducerOptions? QueueOptions { get; set; }
        public ConsumerOptions? ConsumerOptions { get; set; }
        public string RoutingKey { get; set; } = string.Empty;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
    }
}
