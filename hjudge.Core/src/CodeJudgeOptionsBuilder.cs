using System;
using System.Collections.Generic;

namespace hjudge.Core
{
    public sealed class CodeJudgeOptionsBuilder : JudgeOptionsBuilder
    {
        private readonly JudgeOptions judgeOptions = new JudgeOptions();

        public override string GuidStr { get; } = Guid.NewGuid().ToString().Replace("-", string.Empty);

        public override JudgeOptions Build()
        {
            judgeOptions.GuidStr = GuidStr;
            return judgeOptions;
        }

        public override void UseComparingOptions(Action<ComparingOptions>? options = null)
        {
            options?.Invoke(judgeOptions.ComparingOptions);
        }

        public override void UseExtraFiles(List<string> extraFiles)
        {
            judgeOptions.ExtraFiles = extraFiles;
        }

        public override void UseSpecialJudge(Action<SpecialJudgeOptions>? options = null)
        {
            judgeOptions.SpecialJudgeOptions = new SpecialJudgeOptions();
            options?.Invoke(judgeOptions.SpecialJudgeOptions);
        }
        public void UseRunOptions(Action<RunOptions>? options = null)
        {
            options?.Invoke(judgeOptions.RunOptions);
        }

        public void UseStdIO()
        {
            judgeOptions.UseStdIO = true;
        }

        public void AddDataPoint(DataPoint dataPoint)
        {
            judgeOptions.DataPoints.Add(dataPoint);
        }


        public void UseInputFileName(string inputFileName)
        {
            judgeOptions.InputFileName = inputFileName;
        }

        public void UseOutputFileName(string outputFileName)
        {
            judgeOptions.OutputFileName = outputFileName;
        }

        public void UseStdErrBehavior(StdErrBehavior behavior)
        {
            judgeOptions.StandardErrorBehavior = behavior;
        }

        public void UseActiveProcessLimit(int limit)
        {
            judgeOptions.ActiveProcessLimit = limit;
        }
    }
}
