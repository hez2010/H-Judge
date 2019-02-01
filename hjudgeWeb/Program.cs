using hjudgeCore;
using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hjudgeWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //set current directory
            if (args.Length > 0)
            {
                Environment.CurrentDirectory = args[0];
            }

            //Read system and language config from ./AppData/SystemConfig.json and ./AppData/LanguageConfig.json
            SystemConfiguration.Config = JsonConvert.DeserializeObject<SystemConfig>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "AppData", "SystemConfig.json"), Encoding.UTF8));
            Languages.LanguageConfigurations = JsonConvert.DeserializeObject<List<LanguageConfiguration>>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "AppData", "LanguageConfig.json"), Encoding.UTF8));
            
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}
