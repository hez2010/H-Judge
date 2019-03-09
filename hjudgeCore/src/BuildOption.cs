namespace hjudgeCore
{
    public sealed class BuildOption
    {
        public CompilerOption? CompilerOption { get; set; }
        public StaticCheckOption? StaticCheckOption { get; set; }
        public string Source { get; set; } = string.Empty;
        public string ExtensionName { get; set; } = string.Empty;
        public string SubmitFileName { get; set; } = string.Empty;
    }
}
