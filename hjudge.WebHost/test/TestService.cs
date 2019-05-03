using CacheManager.Core;
using EFSecondLevelCache.Core;
using hjudge.Shared.Caching;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpanJson.AspNetCore.Formatter;
using System;

namespace hjudge.WebHost.Test
{
    public static class TestService
    {
        public static IServiceProvider Provider = InitService();
        private static IServiceProvider InitService()
        {
            var services = new ServiceCollection();

            services.AddEFSecondLevelCache();
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration),
               new ConfigurationBuilder()
                       .WithSerializer(typeof(CacheItemJsonSerializer))
                       .WithMicrosoftMemoryCacheHandle(instanceName: "test")
                       .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromHours(4))
                       .Build());

            services.AddDbContext<WebHostDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
                options.EnableDetailedErrors(true);
                options.EnableSensitiveDataLogging(true);
                options.EnableServiceProviderCaching(true);
            });

            services.AddEntityFrameworkInMemoryDatabase();

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IProblemService, ProblemService>();
            services.AddTransient<IContestService, ContestService>();
            services.AddTransient<IJudgeService, JudgeService>();
            services.AddTransient<IGroupService, GroupService>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddSingleton<ILanguageService, LocalLanguageService>();

            services.AddIdentityCore<UserInfo>(options =>
            {
                options.Stores.MaxLengthForKeys = 128;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddSignInManager<SignInManager<UserInfo>>()
            .AddUserManager<CachedUserManager<UserInfo>>()
            .AddEntityFrameworkStores<WebHostDbContext>()
            .AddErrorDescriber<TranslatedIdentityErrorDescriber>();

            services.AddMvc().AddSpanJson();

            return services.BuildServiceProvider();
        }
    }
}
