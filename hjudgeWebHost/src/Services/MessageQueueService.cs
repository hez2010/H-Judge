using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static hjudgeWebHost.Services.MessageQueueFactory;

namespace hjudgeWebHost.Services
{
    public interface IMessageQueueService
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
        public (IModel, ProducerOptions) GetInstance(string queueName)
        {
            return factory.GetProducer(queueName);
        }
    }
}