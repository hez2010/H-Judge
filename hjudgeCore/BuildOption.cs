namespace hjudgeCore
{
    public sealed class BuildOption
    {
        public BuildOption()
        {
            CompilerOption = null;
            StaticCheckOption = null;
        }

        public CompilerOption CompilerOption { get; set; }
        public StaticCheckOption StaticCheckOption { get; set; }
        public string Source { get; set; }
        public string ExtensionName { get; set; }
    }
}
