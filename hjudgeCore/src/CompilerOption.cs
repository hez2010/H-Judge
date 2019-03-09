namespace hjudgeCore
{
    public class CompilerOption
    {
        public string Exec { get; set; } = string.Empty;
        public string Args { get; set; } = string.Empty;
        public bool ReadStdOutput { get; set; } = true;
        public bool ReadStdError { get; set; } = true;
        public string OutputFile { get; set; } = string.Empty;
        public ProblemMatcher? ProblemMatcher { get; set; }
    }
}
