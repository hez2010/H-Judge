using System.Collections.Generic;

namespace hjudge.Core
{
    public class JudgeResult
    {
        public List<JudgePoint>? JudgePoints { get; set; }
        /// <summary>
        /// 格式化后编译日志
        /// </summary>
        public string CompileLog { get; set; } = string.Empty;
        /// <summary>
        /// 格式化后静态检查日志
        /// </summary>
        public string StaticCheckLog { get; set; } = string.Empty;
    }
}
