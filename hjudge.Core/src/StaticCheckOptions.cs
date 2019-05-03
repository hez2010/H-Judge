namespace hjudge.Core
{
    public class StaticCheckOptions
    {
        public string Exec { get; set; } = string.Empty;
        public string Args { get; set; } = string.Empty;
        public bool ReadStdOutput { get; set; } = true;
        public bool ReadStdError { get; set; } = true;
        public ProblemMatcher? ProblemMatcher { get; set; }
    }
}
