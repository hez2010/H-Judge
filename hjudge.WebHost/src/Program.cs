using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace hjudge.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var x = typeof(Program).Assembly.GetReferencedAssemblies().Where(i => i.FullName?.Contains("Json") ?? false).ToList();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
