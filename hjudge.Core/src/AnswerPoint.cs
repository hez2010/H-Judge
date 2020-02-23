using System;

namespace hjudge.Core
{
    public class AnswerPoint : ICloneable
    {
        /// <summary>
        /// 标准答案文件
        /// </summary>
        public string AnswerFile { get; set; } = string.Empty;
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