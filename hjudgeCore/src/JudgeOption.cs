using System.Collections.Generic;

namespace hjudgeCore
{
    public enum StdErrBehavior
    {
        Ignore,
        TreatAsCompileError,
        TreatAsRuntimeError
    }

    public sealed class JudgeOption
    {
        public string GuidStr { get; set; } = string.Empty;
        public ComparingOption ComparingOption { get; set; } = new ComparingOption();
        public RunOption RunOption { get; set; } = new RunOption();
        public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
        public AnswerPoint AnswerPoint { get; set; } = new AnswerPoint();
        public List<string> ExtraFiles { get; set; } = new List<string>();
        public SpecialJudgeOption? SpecialJudgeOption { get; set; }
        public string InputFileName { get; set; } = string.Empty;
        public string OutputFileName { get; set; } = string.Empty;
        public bool UseStdIO { get; set; } = true;
        public StdErrBehavior StandardErrorBehavior { get; set; } = StdErrBehavior.Ignore;
        public int ActiveProcessLimit { get; set; } = 1;
    }
}
