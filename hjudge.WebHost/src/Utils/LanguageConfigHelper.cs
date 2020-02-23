using System.Collections.Generic;
using System.Linq;
using hjudge.WebHost.Configurations;
using hjudge.WebHost.Models.Language;

namespace hjudge.WebHost.Utils
{
    public static class LanguageConfigHelper
    {
        public static IEnumerable<LanguageModel> GenerateLanguageConfig(IEnumerable<LanguageConfig> langConfig, string[]? languages)
        {
            foreach (var i in langConfig)
            {
                if (languages is null || languages.Length == 0 || languages.Contains(i.Name))
                {
                    yield return new LanguageModel
                    {
                        Name = i.Name,
                        Information = i.Information,
                        SyntaxHighlight = i.SyntaxHighlight
                    };
                }
            }
        }

    }
}
