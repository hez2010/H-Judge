namespace hjudgeCore
{
    public class SpecialJudgeOption
    {
        public SpecialJudgeOption()
        {
            UseStdInputFile = UseStdOutputFile = UseOutputFile = true;
        }

        public string Exec { get; set; }
        public bool UseStdInputFile { get; set; }
        public bool UseStdOutputFile { get; set; }
        public bool UseOutputFile { get; set; }
    }
}
