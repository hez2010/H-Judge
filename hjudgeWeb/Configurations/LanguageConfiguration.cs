using System.Collections.Generic;
using System.Linq;

namespace hjudgeWeb.Configurations
{
    public class LanguageConfiguration
    {
        public string Name { get; set; }
        public string Extensions { get; set; }
        public string CompilerExec { get; set; }
        public string CompilerArgs { get; set; }
        public string CompilerProblemMatcher { get; set; }
        public string CompilerDisplayFormat { get; set; }
        public string StaticCheckExec { get; set; }
        public string StaticCheckArgs { get; set; }
        public string StaticCheckProblemMatcher { get; set; }
        public string StaticCheckDisplayFormat { get; set; }
        public string Exec { get; set; }
        public string Args { get; set; }
    }

    public class Languages
    {
        public static List<LanguageConfiguration> LanguageConfigurations { get; set; } = new List<LanguageConfiguration>();
        public static List<string> LanguagesList => LanguageConfigurations.Select(i => i.Name).ToList();
    }
}
