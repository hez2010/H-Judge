using System.Collections.Generic;

namespace hjudge.Core
{
    public enum StdErrBehavior : int
    {
        Ignore,
        TreatAsCompileError,
        TreatAsRuntimeError
    }

    public sealed class JudgeOptions
    {
        public string GuidStr { get; set; } = string.Empty;
        public ComparingOptions ComparingOptions { get; set; } = new ComparingOptions();
        public RunOptions RunOptions { get; set; } = new RunOptions();
        public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
        public AnswerPoint? AnswerPoint { get; set; }
        public List<string> ExtraFiles { get; set; } = new List<string>();
        public SpecialJudgeOptions? SpecialJudgeOptions { get; set; }
        public string InputFileName { get; set; } = string.Empty;
        public string OutputFileName { get; set; } = string.Empty;
        public bool UseStdIO { get; set; } = true;
        public StdErrBehavior StandardErrorBehavior { get; set; } = StdErrBehavior.Ignore;
        public int ActiveProcessLimit { get; set; } = 1;
    }
}
