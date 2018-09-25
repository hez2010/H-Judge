using System;

namespace hjudgeCore
{
    public class AnswerPoint : ICloneable
    {
        public AnswerPoint()
        {
            Score = 0;
        }

        public string AnswerFile { get; set; }
        public float Score { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}