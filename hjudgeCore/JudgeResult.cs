using System.Collections.Generic;

namespace hjudgeCore
{
    public class JudgeResult
    {
        public List<JudgePoint> JudgePoints { get; set; }
        public string CompileLog { get; set; }
        public string StaticCheckLog { get; set; }
    }
}
