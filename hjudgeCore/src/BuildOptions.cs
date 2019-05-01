namespace hjudgeCore
{
    public sealed class BuildOptions
    {
        public CompilerOptions? CompilerOption { get; set; }
        public StaticCheckOptions? StaticCheckOption { get; set; }
        public string Source { get; set; } = string.Empty;
        public string ExtensionName { get; set; } = string.Empty;
        public string SubmitFileName { get; set; } = string.Empty;
    }
}
