using hjudgeCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace hjudgeJudgeHost
{
    public class JudgeInfo
    {
        public int Id { get; set; }
        public DateTime JudgeTime { get; set; }
        public BuildOption? BuildOption { get; set; }
        public JudgeOption? JudgeOption { get; set; }
    }
}
