using hjudge.WebHost.Configurations;
using hjudge.Shared.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace hjudge.WebHost.Services
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
        private List<LanguageConfig> languageConfigs = new List<LanguageConfig>();
        private const string fileName = "./AppData/LanguageConfig.json";
        private readonly FileSystemWatcher watcher = new FileSystemWatcher("./AppData");
        public LocalLanguageService()
        {
            LoadLanguageConfig();
            watcher.IncludeSubdirectories = false;
            watcher.Changed += (_, e) => { if (e.ChangeType == WatcherChangeTypes.Changed) LoadLanguageConfig(); };
            watcher.EnableRaisingEvents = true;
        }

        private void LoadLanguageConfig()
        {
            try
            {
                var data = File.Exists(fileName) ? File.ReadAllBytes(fileName) : Encoding.UTF8.GetBytes("[]");
                languageConfigs.Clear();
                languageConfigs = data.DeserializeJson<List<LanguageConfig>>(false);
            }
            catch { /* ignored */ }
        }

        public async Task<bool> AddLanguageConfigAsync(LanguageConfig config)
        {
            if (languageConfigs.Any(i => i.Name == config.Name)) return false;
            languageConfigs.Add(config);
            watcher.EnableRaisingEvents = false;
            await File.WriteAllBytesAsync(fileName, languageConfigs.SerializeJson(false));
            watcher.EnableRaisingEvents = true;
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
                watcher.EnableRaisingEvents = false;
                await File.WriteAllBytesAsync(fileName, languageConfigs.SerializeJson(false));
                watcher.EnableRaisingEvents = true;
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
                watcher.EnableRaisingEvents = false;
                await File.WriteAllBytesAsync(fileName, languageConfigs.SerializeJson(false));
                watcher.EnableRaisingEvents = true;
                return true;
            }
            return false;
        }
    }
}
