using System;

namespace hjudge.Core
{
    public class DataPoint : ICloneable
    {
        /// <summary>
        /// 标准输入文件
        /// </summary>
        public string StdInFile { get; set; } = string.Empty;
        /// <summary>
        /// 标准输出文件
        /// </summary>
        public string StdOutFile { get; set; } = string.Empty;
        /// <summary>
        /// 时间限制，单位：毫秒
        /// </summary>
        public long TimeLimit { get; set; } = 1000;
        /// <summary>
        /// 内存限制，单位：千字节
        /// </summary>
        public long MemoryLimit { get; set; } = 131072;
        /// <summary>
        /// 分数
        /// </summary>
        public float Score { get; set; } = 0;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}