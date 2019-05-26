namespace hjudge.JudgeHost
{
    class JudgeHostConfig
    {
        public MessageQueueOptions? MessageQueue { get; set; }
        public int ConcurrentJudgeTask { get; set; }
    }
}
