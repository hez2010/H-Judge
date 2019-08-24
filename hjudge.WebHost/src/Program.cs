using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudge.WebHost
{
    public class Program
    {
        public static IServiceProvider? RootServiceProvider;
        public static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            RootServiceProvider = host.Services;
            return host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
