using System;

namespace hjudge.Core
{
    public sealed class BuildOptionsBuilder
    {
        private readonly BuildOptions buildOptions = new BuildOptions();

        public void UseCompiler(Action<CompilerOptions>? options = null)
        {
            var compilerOptions = new CompilerOptions();
            options?.Invoke(compilerOptions);
            buildOptions.CompilerOption = compilerOptions;
        }

        public void UseStaticCheck(Action<StaticCheckOptions>? options = null)
        {
            var staticCheckOptions = new StaticCheckOptions();
            options?.Invoke(staticCheckOptions);
            buildOptions.StaticCheckOption = staticCheckOptions;
        }

        public void AddSource(string content, string fileName)
        {
            buildOptions.SourceFiles.Add(new Source
            {
                FileName = fileName,
                Content = content
            });
        }

        public BuildOptions Build()
        {
            return buildOptions;
        }
    }
}
