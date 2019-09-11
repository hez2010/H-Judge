using hjudge.Core;

namespace hjudge.WebHost.Configurations
{
    public class LanguageConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Information { get; set; } = string.Empty;
        public string Extensions { get; set; } = string.Empty;
        public string SyntaxHighlight { get; set; } = string.Empty;
        //Compiler
        public string CompilerExec { get; set; } = string.Empty;
        public string CompilerArgs { get; set; } = string.Empty;
        public string CompilerProblemMatcher { get; set; } = string.Empty;
        public string CompilerDisplayFormat { get; set; } = string.Empty;
        public bool CompilerReadStdOutput { get; set; }
        public bool CompilerReadStdError { get; set; } = true;
        //Static check
        public string StaticCheckExec { get; set; } = string.Empty;
        public string StaticCheckArgs { get; set; } = string.Empty;
        public string StaticCheckProblemMatcher { get; set; } = string.Empty;
        public string StaticCheckDisplayFormat { get; set; } = string.Empty;
        public bool StaticCheckReadStdOutput { get; set; }
        public bool StaticCheckReadStdError { get; set; } = true;
        //Run option
        public string RunExec { get; set; } = string.Empty;
        public string RunArgs { get; set; } = string.Empty;
        public int ActiveProcessLimit { get; set; } = 1;
        public StdErrBehavior StandardErrorBehavior { get; set; } = StdErrBehavior.Ignore;
    }
}
