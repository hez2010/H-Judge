using System;
using System.Collections.Generic;

namespace hjudgeCore
{
    public sealed class AnswerJudgeOptionBuilder : JudgeOptionBuilder
    {
        private readonly JudgeOption judgeOption = new JudgeOption();

        public override string GuidStr { get; } = Guid.NewGuid().ToString().Replace("-", string.Empty);

        public override JudgeOption Build()
        {
            judgeOption.GuidStr = GuidStr;
            return judgeOption;
        }

        public override void UseComparingOption(Action<ComparingOption>? option = null)
        {
            var comparingOption = new ComparingOption();
            option?.Invoke(comparingOption);
            judgeOption.ComparingOption = comparingOption;
        }

        public override void UseExtraFiles(List<string> extraFiles)
        {
            judgeOption.ExtraFiles = extraFiles;
        }

        public override void UseSpecialJudge(Action<SpecialJudgeOption>? option = null)
        {
            var specialJudgeOption = new SpecialJudgeOption();
            option?.Invoke(specialJudgeOption);
            judgeOption.SpecialJudgeOption = specialJudgeOption;
        }

        public void UseAnswerPoint(AnswerPoint answerPoint)
        {
            judgeOption.AnswerPoint = answerPoint;
        }
    }
}
