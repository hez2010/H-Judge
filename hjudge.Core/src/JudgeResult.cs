using System.Collections.Generic;

namespace hjudge.Core
{
    public class JudgeResult
    {
        public List<JudgePoint>? JudgePoints { get; set; }
        public string CompileLog { get; set; } = string.Empty;
        public string StaticCheckLog { get; set; } = string.Empty;
    }
}
