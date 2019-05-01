using System;
using System.Collections.Generic;

namespace hjudgeCore
{
    public abstract class JudgeOptionsBuilder
    {
        public abstract void UseComparingOptions(Action<ComparingOptions>? options = null);
        public abstract void UseExtraFiles(List<string> extraFiles);
        public abstract void UseSpecialJudge(Action<SpecialJudgeOptions>? options = null);
        public abstract string GuidStr { get; }

        public abstract JudgeOptions Build();
    }
}
