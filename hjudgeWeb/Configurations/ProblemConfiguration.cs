using hjudgeCore;
using System.Collections.Generic;
using System.Linq;

namespace hjudgeWeb.Configurations
{
    public class ProblemConfiguration
    {
        public ProblemConfiguration()
        {
            ExtraFiles = new List<string>();
            Points = new List<DataPoint>();
            ComparingOptions = new ComparingOption();
            Answer = new AnswerPoint();
            UseStdIO = true;
        }

        public string SpecialJudge { get; set; }
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
        public List<string> ExtraFiles { get; set; }
        public List<DataPoint> Points { get; set; }
        public AnswerPoint Answer { get; set; }
        public ComparingOption ComparingOptions { get; set; }
        public bool UseStdIO { get; set; }
        public string CompileArgs { get; set; }
        public string ExtraFilesText => ExtraFiles.Aggregate(string.Empty, (accu, next) => accu + "\n" + next);
    }
}
