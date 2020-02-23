using System;
using hjudge.Shared.MessageQueue;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using static hjudge.Shared.MessageQueue.MessageQueueFactory;

namespace hjudge.WebHost.Services
{
    public interface IMessageQueueService : IDisposable
    {
        (IModel Channel, ProducerOptions Options) GetInstance(string queueName);
    }

    public class MessageQueueServiceOptions
    {
        public MessageQueueFactory? MessageQueueFactory { get; set; }
    }

    public class MessageQueueService : IMessageQueueService
    {
        private readonly MessageQueueFactory factory;

        public MessageQueueService(IOptions<MessageQueueServiceOptions> options)
        {
            factory = options.Value.MessageQueueFactory ?? throw new NullReferenceException();
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                factory.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public (IModel, ProducerOptions) GetInstance(string queueName)
        {
            return factory.GetProducer(queueName);
        }
    }
}