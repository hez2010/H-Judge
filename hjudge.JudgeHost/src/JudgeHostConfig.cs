namespace hjudge.JudgeHost
{
    class JudgeHostConfig
    {
        public MessageQueueOptions? MessageQueue { get; set; }
        public string FileHost { get; set; } = string.Empty;
        public string DataCacheDirectory { get; set; } = string.Empty;
    }
}
