using System;
using hjudge.FileHost.Data;
using hjudge.FileHost.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace hjudge.FileHost.Test
{
    class TestService
    {
        public static IServiceProvider Provider = InitService();
        private static IServiceProvider InitService()
        {
            var services = new ServiceCollection();

            services.AddDbContext<FileHostDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.EnableServiceProviderCaching();
            });

            services.AddEntityFrameworkInMemoryDatabase();

            services.AddScoped<SeaweedFsService>()
                .Configure<SeaweedFsOptions>(options =>
                {
                    options.MasterHostName = "localhost";
                    options.Port = 9333;
                });

            return services.BuildServiceProvider();
        }
    }
}
