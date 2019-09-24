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

        public void Dispose()
        {
            factory.Dispose();
        }

        public (IModel, ProducerOptions) GetInstance(string queueName)
        {
            return factory.GetProducer(queueName);
        }
    }
}