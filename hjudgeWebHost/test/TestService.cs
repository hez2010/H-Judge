using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpanJson.AspNetCore.Formatter;
using System;

namespace hjudgeWebHostTest
{
    public static class TestService
    {
        public static IServiceProvider Provider = InitService();
        private static IServiceProvider InitService()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
                options.EnableDetailedErrors(true);
                options.EnableSensitiveDataLogging(true);
                options.EnableServiceProviderCaching(true);
            });

            services.AddEntityFrameworkInMemoryDatabase();

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "127.0.0.1";
                options.InstanceName = "hjudge";
            });

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
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<TranslatedIdentityErrorDescriber>();

            services.AddMvc().AddSpanJson();

            return services.BuildServiceProvider();
        }
    }
}
