using System;

namespace hjudgeCore
{
    public class DataPoint : ICloneable
    {
        public DataPoint()
        {
            TimeLimit = 1000;
            MemoryLimit = 131072;
            Score = 0;
        }

        public string StdInFile { get; set; }
        public string StdOutFile { get; set; }
        public long TimeLimit { get; set; }
        public long MemoryLimit { get; set; }
        public float Score { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}