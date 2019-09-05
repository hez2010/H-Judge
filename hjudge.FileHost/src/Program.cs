using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace hjudge.FileHost
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(kestrelOptions =>
                    {
                        // TODO: load port from config
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
