using hjudge.Shared.MessageQueue;

namespace hjudge.JudgeHost
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
}
