using hjudgeWebHost.Configurations;
using SpanJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface ILanguageService
    {
        Task<IEnumerable<LanguageConfig>> GetLanguageConfigAsync();
        Task<bool> AddLanguageConfigAsync(LanguageConfig config);
        Task<bool> RemoveLanguageConfigAsync(LanguageConfig config);
        Task<bool> UpdateLanguageConfigAsync(LanguageConfig config);
    }
    public class LocalLanguageService : ILanguageService
    {
        private readonly List<LanguageConfig> languageConfigs;
        private readonly string fileName;
        public LocalLanguageService()
        {
            fileName = "./AppData/LanguageConfig.json";
            languageConfigs = JsonSerializer.Generic.Utf8.Deserialize<List<LanguageConfig>>(Encoding.UTF8.GetBytes(File.ReadAllText(fileName)));
        }

        public async Task<bool> AddLanguageConfigAsync(LanguageConfig config)
        {
            if (languageConfigs.Any(i => i.Name == config.Name)) return false;

            languageConfigs.Add(config);
            await File.WriteAllBytesAsync(fileName, JsonSerializer.Generic.Utf8.Serialize(languageConfigs));
            return true;
        }

        public Task<IEnumerable<LanguageConfig>> GetLanguageConfigAsync()
        {
            return Task.FromResult(languageConfigs.AsEnumerable());
        }

        public async Task<bool> RemoveLanguageConfigAsync(LanguageConfig config)
        {
            var lang = languageConfigs.FirstOrDefault(i => i.Name != config.Name);
            if (lang == null) return false;

            if (languageConfigs.Remove(lang))
            {
                await File.WriteAllBytesAsync(fileName, JsonSerializer.Generic.Utf8.Serialize(languageConfigs));
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateLanguageConfigAsync(LanguageConfig config)
        {
            var lang = languageConfigs.FirstOrDefault(i => i.Name != config.Name);
            if (lang == null) return false;

            if (languageConfigs.Remove(lang))
            {
                languageConfigs.Add(config);
                await File.WriteAllBytesAsync(fileName, JsonSerializer.Generic.Utf8.Serialize(languageConfigs));
                return true;
            }
            return false;
        }
    }
}
