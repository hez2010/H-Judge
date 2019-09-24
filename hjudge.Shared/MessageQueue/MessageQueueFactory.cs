using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace hjudge.Shared.MessageQueue
{
    public class MessageQueueFactory : IDisposable
    {
        private readonly ConcurrentDictionary<string, (IConnection Connection, IModel Model, ProducerOptions Options)> producers = new ConcurrentDictionary<string, (IConnection, IModel, ProducerOptions)>();
        private readonly ConcurrentBag<(IConnection Connection, AsyncEventingBasicConsumer Consumer, ConsumerOptions Options)> consumers = new ConcurrentBag<(IConnection, AsyncEventingBasicConsumer, ConsumerOptions)>();

        private readonly ConnectionFactory factory;

        public class HostOptions
        {
            public string HostName { get; set; } = string.Empty;
            public string VirtualHost { get; set; } = "/";
            public int Port { get; set; } = 5672;
            public string UserName { get; set; } = "guest";
            public string Password { get; set; } = "guest";
        }

        public MessageQueueFactory(HostOptions options)
        {
            factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true
            };
        }

        public class ProducerOptions
        {
            public string Queue { get; set; } = string.Empty;
            public bool Durable { get; set; } = true;
            public bool Exclusive { get; set; } = false;
            public bool AutoDelete { get; set; } = false;
            public string Exchange { get; set; } = string.Empty;
            public string RoutingKey { get; set; } = string.Empty;
        }

        public void CreateProducer(ProducerOptions options)
        {
            if (producers.ContainsKey(options.Queue)) throw new InvalidOperationException($"Queue {options.Queue} already exists.");

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: options.Queue,
                durable: options.Durable,
                exclusive: options.Exclusive,
                autoDelete: options.AutoDelete);

            channel.ExchangeDeclare(
                exchange: options.Exchange,
                durable: options.Durable,
                autoDelete: options.AutoDelete,
                type: ExchangeType.Direct);

            channel.QueueBind(options.Queue, options.Exchange, options.RoutingKey);

            producers.TryAdd(options.Queue, (connection, channel, options));
        }

        public (IModel Channel, ProducerOptions Options) GetProducer(string queue)
        {
            if (producers.TryGetValue(queue, out var p)) return (p.Model, p.Options);
            throw new InvalidOperationException($"No producer queue named {queue}.");
        }

        public class ConsumerOptions
        {
            public string Queue { get; set; } = string.Empty;
            public bool Durable { get; set; } = true;
            public bool AutoAck { get; set; } = false;
            public bool Exclusive { get; set; } = false;
            public string Exchange { get; set; } = string.Empty;
            public string RoutingKey { get; set; } = string.Empty;

            [IgnoreDataMember]
            public AsyncEventHandler<BasicDeliverEventArgs>? OnReceived { get; set; }
            [IgnoreDataMember]
            public AsyncEventHandler<ConsumerEventArgs>? OnRegistered { get; set; }
            [IgnoreDataMember]
            public AsyncEventHandler<ConsumerEventArgs>? OnCancelled { get; set; }
            [IgnoreDataMember]
            public AsyncEventHandler<ShutdownEventArgs>? OnShutdown { get; set; }
            [IgnoreDataMember]
            public AsyncEventHandler<ConsumerEventArgs>? OnUnregistered { get; set; }
        }

        public void CreateConsumer(ConsumerOptions options)
        {
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: options.Queue,
                durable: options.Durable,
                exclusive: options.Exclusive,
                autoDelete: false);

            channel.ExchangeDeclare(
                exchange: options.Exchange,
                durable: options.Durable,
                autoDelete: false,
                type: ExchangeType.Direct);

            channel.QueueBind(options.Queue, options.Exchange, options.RoutingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);

            if (options.OnReceived != null) consumer.Received += options.OnReceived;
            if (options.OnRegistered != null) consumer.Registered += options.OnRegistered;
            if (options.OnUnregistered != null) consumer.Unregistered += options.OnUnregistered;
            if (options.OnShutdown != null) consumer.Shutdown += options.OnShutdown;
            if (options.OnCancelled != null) consumer.ConsumerCancelled += options.OnCancelled;

            channel.BasicConsume(consumer, options.Queue, options.AutoAck, Guid.NewGuid().ToString(), false, options.Exclusive);

            consumers.Add((connection, consumer, options));
        }

        public void Dispose()
        {
            while (consumers.TryTake(out var c))
            {
                c.Consumer.Model.Close();
                c.Connection.Close();
                c.Consumer.Model.Dispose();
                c.Connection.Dispose();
            }

            foreach (var p in producers)
            {
                p.Value.Model.Close();
                p.Value.Connection.Close();
                p.Value.Model.Dispose();
                p.Value.Connection.Dispose();
            }
            producers.Clear();
        }
    }
}
