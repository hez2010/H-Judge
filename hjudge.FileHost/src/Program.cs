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
                            var port = 61726;
                            if (args != null)
                            {
                                foreach (var i in args)
                                {
                                    if (i.StartsWith("--"))
                                    {
                                        var split = i.IndexOf("=");
                                        var command = i[2..split].ToLowerInvariant();
                                        var value = i[(split + 1)..];

                                        switch (command)
                                        {
                                            case "port":
                                                port = int.Parse(value);
                                                break;
                                        }
                                    }
                                }
                            }
                            kestrelOptions.ListenLocalhost(port,
                                listenOptions =>
                                {
                                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core
                                        .HttpProtocols.Http2;
                                });
                        });
                });
    }
}
