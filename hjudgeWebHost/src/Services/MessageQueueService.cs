using hjudgeShared.MessageQueue;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using static hjudgeShared.MessageQueue.MessageQueueFactory;

namespace hjudgeWebHost.Services
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
            factory = options.Value.MessageQueueFactory ?? throw new ArgumentNullException("MessageQueueFactory");
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