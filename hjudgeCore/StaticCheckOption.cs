namespace hjudgeCore
{
    public class StaticCheckOption
    {
        public StaticCheckOption()
        {
            ReadStdOutput = ReadStdError = true;
        }

        public string Exec { get; set; }
        public string Args { get; set; }
        public bool ReadStdOutput { get; set; }
        public bool ReadStdError { get; set; }
        public ProblemMatcher ProblemMatcher { get; set; }
    }
}
