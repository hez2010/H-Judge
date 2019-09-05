using System;
using CacheManager.Core;
using EFSecondLevelCache.Core;
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

            services.AddEFSecondLevelCache();
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration),
               new ConfigurationBuilder()
                       .WithJsonSerializer()
                       .WithMicrosoftMemoryCacheHandle(instanceName: "test")
                       .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromHours(4))
                       .Build());

            return services.BuildServiceProvider();
        }
    }
}
