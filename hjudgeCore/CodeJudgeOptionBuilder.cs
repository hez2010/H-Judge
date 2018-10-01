using System;
using System.Collections.Generic;

namespace hjudgeCore
{
    public sealed class CodeJudgeOptionBuilder : JudgeOptionBuilder
    {
        private JudgeOption judgeOption = new JudgeOption();

        public override string GuidStr { get; } = Guid.NewGuid().ToString().Replace("-", string.Empty);

        public override JudgeOption Build()
        {
            judgeOption.GuidStr = GuidStr;
            return judgeOption;
        }

        public override void UseComparingOption(Action<ComparingOption> option = null)
        {
            var comparingOption = new ComparingOption();
            option?.Invoke(comparingOption);
            judgeOption.ComparingOption = comparingOption;
        }

        public override void UseExtraFiles(List<string> extraFiles)
        {
            judgeOption.ExtraFiles = extraFiles;
        }

        public override void UseSpecialJudge(Action<SpecialJudgeOption> option = null)
        {
            var specialJudgeOption = new SpecialJudgeOption();
            option?.Invoke(specialJudgeOption);
            judgeOption.SpecialJudgeOption = specialJudgeOption;
        }

        public void UseStdIO()
        {
            judgeOption.UseStdIO = true;
        }

        public void AddDataPoint(DataPoint dataPoint)
        {
            judgeOption.DataPoints.Add(dataPoint);
        }

        public void SetRunOption(Action<RunOption> option = null)
        {
            var runOption = new RunOption();
            option?.Invoke(runOption);
            judgeOption.RunOption = runOption;
        }

        public void SetInputFileName(string inputFileName)
        {
            judgeOption.InputFileName = inputFileName;
        }

        public void SetOutputFileName(string outputFileName)
        {
            judgeOption.OutputFileName = outputFileName;
        }
    }
}
