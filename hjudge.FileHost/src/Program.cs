using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace hjudge.FileHost
{
    class Program
    {
        static Task Main(string[] args) => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder
                        .ConfigureKestrel(kestrelOptions =>
                        {
                            // TODO: int.Parse(configuration["Host:Port"])
                            kestrelOptions.ListenLocalhost(61726,
                                listenOptions =>
                                {
                                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core
                                        .HttpProtocols.Http2;
                                });
                        });
                });
    }
}
