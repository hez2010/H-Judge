namespace hjudgeCore
{
    public class SpecialJudgeOptions
    {
        public string Exec { get; set; } = string.Empty;
        public bool UseSourceFile { get; set; } = true;
        public bool UseStdInputFile { get; set; } = true;
        public bool UseStdOutputFile { get; set; } = true;
        public bool UseOutputFile { get; set; } = true;
    }
}
