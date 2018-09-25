namespace hjudgeCore
{
    public class JudgePoint
    {
        public float Score { get; set; }
        public long TimeCost { get; set; }
        public long MemoryCost { get; set; }
        public int ExitCode { get; set; }
        public string ExtraInfo { get; set; }
        public ResultCode Result { get; set; }
    }
}
