using System.Collections.Generic;

namespace hjudgeCore
{
    public sealed class JudgeOption
    {
        public JudgeOption()
        {
            DataPoints = new List<DataPoint>();
        }

        public string GuidStr { get; set; }
        public ComparingOption ComparingOption { get; set; }
        public RunOption RunOption { get; set; }
        public List<DataPoint> DataPoints { get; set; }
        public AnswerPoint AnswerPoint { get; set; }
        public List<string> ExtraFiles { get; set; }
        public SpecialJudgeOption SpecialJudgeOption { get; set; }
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
        public bool UseStdIO { get; set; }
    }
}
