using System;
using System.Collections.Generic;

namespace hjudge.Core
{
    public sealed class AnswerJudgeOptionsBuilder : JudgeOptionsBuilder
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
            var specialJudgeOptions = new SpecialJudgeOptions();
            options?.Invoke(specialJudgeOptions);
            judgeOptions.SpecialJudgeOptions = specialJudgeOptions;
        }

        public void UseAnswerPoint(AnswerPoint answerPoint)
        {
            judgeOptions.AnswerPoint = answerPoint;
        }
    }
}
