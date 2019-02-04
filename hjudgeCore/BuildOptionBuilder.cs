using System;

namespace hjudgeCore
{
    public sealed class BuildOptionBuilder
    {
        private readonly BuildOption buildOption = new BuildOption();

        public void UseCompiler(Action<CompilerOption> option = null)
        {
            var compilerOption = new CompilerOption();
            option?.Invoke(compilerOption);
            buildOption.CompilerOption = compilerOption;
        }

        public void UseStaticCheck(Action<StaticCheckOption> option = null)
        {
            var staticCheckOption = new StaticCheckOption();
            option?.Invoke(staticCheckOption);
            buildOption.StaticCheckOption = staticCheckOption;
        }

        public void UseCustomSubmitFileName(string fileName)
        {
            buildOption.SubmitFileName = fileName;
        }

        public void AddSource(string source)
        {
            buildOption.Source = source;
        }

        public void AddExtensionName(string extensionName)
        {
            buildOption.ExtensionName = extensionName;
        }

        public BuildOption Build()
        {
            return buildOption;
        }
    }
}
