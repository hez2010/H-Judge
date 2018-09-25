namespace hjudgeCore
{
    public class CompilerOption
    {
        public CompilerOption()
        {
            ReadStdOutput = ReadStdError = true;
        }

        public string Exec { get; set; }
        public string Args { get; set; }
        public bool ReadStdOutput { get; set; }
        public bool ReadStdError { get; set; }
        public string OutputFile { get; set; }
        public ProblemMatcher ProblemMatcher { get; set; }
    }
}
