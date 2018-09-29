using hjudgeWeb.Configurations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace hjudgeWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Languages.LanguageConfigurations.Add(new LanguageConfiguration
            {
                Name = "C++"
            });
            Languages.LanguageConfigurations.Add(new LanguageConfiguration
            {
                Name = "C"
            });
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}
