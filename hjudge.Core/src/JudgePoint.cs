using System;

namespace hjudge.Core
{
    public class JudgePoint
    {
        /// <summary>
        /// 得分
        /// </summary>
        public float Score { get; set; }
        /// <summary>
        /// 用时，单位：毫秒
        /// </summary>
        public long TimeCost { get; set; }
        /// <summary>
        /// 用存，单位：千字节
        /// </summary>
        public long MemoryCost { get; set; }
        /// <summary>
        /// 退出代码
        /// </summary>
        public int ExitCode { get; set; }
        /// <summary>
        /// 额外信息
        /// </summary>
        public string ExtraInfo { get; set; } = string.Empty;
        /// <summary>
        /// 结果类型
        /// </summary>
        public ResultCode ResultType { get; set; }
        /// <summary>
        /// 结果文本
        /// </summary>
        public string Result => Enum.GetName(typeof(ResultCode), ResultType)?.Replace("_", " ") ?? "Unknown Error";
    }
}
