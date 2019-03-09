using System;

namespace hjudgeCore
{
    public class DataPoint : ICloneable
    {
        public string StdInFile { get; set; } = string.Empty;
        public string StdOutFile { get; set; } = string.Empty;
        public long TimeLimit { get; set; } = 1000;
        public long MemoryLimit { get; set; } = 131072;
        public float Score { get; set; } = 0;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}