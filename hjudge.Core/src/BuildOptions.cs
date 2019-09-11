using System.Collections.Generic;

namespace hjudge.Core
{
    public sealed class BuildOptions
    {
        public CompilerOptions? CompilerOption { get; set; }
        public StaticCheckOptions? StaticCheckOption { get; set; }
        public List<Source> SourceFiles { get; set; } = new List<Source>();
    }
}
