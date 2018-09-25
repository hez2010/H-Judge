using System;
using System.Collections.Generic;

namespace hjudgeCore
{
    public abstract class JudgeOptionBuilder
    {
        public abstract void UseComparingOption(Action<ComparingOption> option = null);
        public abstract void UseExtraFiles(List<string> extraFiles);
        public abstract void UseSpecialJudge(Action<SpecialJudgeOption> option = null);
        public abstract void AttachContest(int contestId);

        public abstract JudgeOption Build();
    }
}
