using System;

namespace hjudge.Core
{
    public class JudgePoint
    {
        public float Score { get; set; }
        public long TimeCost { get; set; }
        public long MemoryCost { get; set; }
        public int ExitCode { get; set; }
        public string ExtraInfo { get; set; } = string.Empty;
        public ResultCode ResultType { get; set; }
        public string Result => Enum.GetName(typeof(ResultCode), ResultType).Replace("_", " ");
    }
}
