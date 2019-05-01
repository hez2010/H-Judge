using System;

namespace hjudgeCore
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

        public void UseCustomSubmitFileName(string fileName)
        {
            buildOptions.SubmitFileName = fileName;
        }

        public void UseSource(string source)
        {
            buildOptions.Source = source;
        }

        public void UseExtensionName(string extensionName)
        {
            buildOptions.ExtensionName = extensionName;
        }

        public BuildOptions Build()
        {
            return buildOptions;
        }
    }
}
