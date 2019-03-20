using hjudgeCore;
using System.Collections.Generic;
using System.Linq;

namespace hjudgeWebHost.Configurations
{
    public class ProblemConfig
    {
        public ProblemConfig()
        {
            ExtraFiles = new List<string>();
            Points = new List<DataPoint>();
            ComparingOptions = new ComparingOption();
            Answer = new AnswerPoint();
            UseStdIO = true;
        }

        public string SpecialJudge { get; set; } = string.Empty;
        public string InputFileName { get; set; } = string.Empty;
        public string OutputFileName { get; set; } = string.Empty;
        public string SubmitFileName { get; set; } = string.Empty;
        public List<string> ExtraFiles { get; set; }
        public List<DataPoint> Points { get; set; }
        public AnswerPoint Answer { get; set; }
        public ComparingOption ComparingOptions { get; set; }
        public bool UseStdIO { get; set; }
        public string CompileArgs { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string ExtraFilesText => ExtraFiles.Aggregate(string.Empty, (accu, next) => accu + next + "\n");
    }
}
