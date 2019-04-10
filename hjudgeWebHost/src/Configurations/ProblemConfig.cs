using hjudgeCore;
using System.Collections.Generic;
using System.Linq;

namespace hjudgeWebHost.Configurations
{
    public class ProblemConfig
    {
        public string SpecialJudge { get; set; } = string.Empty;
        public string InputFileName { get; set; } = string.Empty;
        public string OutputFileName { get; set; } = string.Empty;
        public string SubmitFileName { get; set; } = string.Empty;
        public List<string> ExtraFiles { get; set; } = new List<string>();
        public List<DataPoint> Points { get; set; } = new List<DataPoint>();
        public AnswerPoint Answer { get; set; } = new AnswerPoint();
        public ComparingOption ComparingOptions { get; set; } = new ComparingOption();
        public bool UseStdIO { get; set; } = true;
        public string CompileArgs { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string ExtraFilesText => ExtraFiles.Aggregate(string.Empty, (accu, next) => accu + next + "\n");
        public long CodeSizeLimit { get; set; }
    }
}
