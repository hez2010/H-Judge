using hjudgeCore;
using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public static readonly DbContextOptionsBuilder<ApplicationDbContext> DbContextOptionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();
        
        public static async Task Main(string[] args)
        {
            var systemConfig = JObject.Parse(File.ReadAllText("AppData/SystemConfig.json", Encoding.UTF8));
            SystemConfiguration.Environments = systemConfig["Environments"].ToString();
            Languages.LanguageConfigurations = JsonConvert.DeserializeObject<List<LanguageConfiguration>>(File.ReadAllText("AppData/LanguageConfig.json", Encoding.UTF8));

            var connectionString = JsonConvert
                .DeserializeAnonymousType(File.ReadAllText("appsettings.json"),
                    new { ConnectionStrings = new { DefaultConnection = "" } }).ConnectionStrings.DefaultConnection;
            DbContextOptionsBuilder.UseSqlServer(connectionString);

            using (var db = new ApplicationDbContext(DbContextOptionsBuilder.Options))
            {
                foreach (var i in db.Judge.Where(i => i.ResultType == (int)ResultCode.Judging))
                {
                    i.ResultType = (int)ResultCode.Unknown_Error;
                }

                await db.SaveChangesAsync();
                foreach (var i in db.Judge.Where(i => i.ResultType == (int)ResultCode.Pending).Select(i => i.Id))
                {
                    JudgeQueue.JudgeIdQueue.Enqueue(i);
                }
            }

            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                new Thread(async () => await JudgeQueue.JudgeThread()).Start();
            }

            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}
