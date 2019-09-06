using CacheManager.Core;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using EFSecondLevelCache.Core;
using hjudge.WebHost.Extensions;
using hjudge.WebHost.Middlewares;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;
using React.AspNet;

namespace hjudge.WebHost.Test
{
    public static class TestService
    {
        private static readonly IServiceProvider provider = InitService();
        public static IServiceProvider Provider => provider.CreateScope().ServiceProvider;
        private static IServiceProvider InitService()
        {
            var services = new ServiceCollection();

            //services.AddEFSecondLevelCache();
            services.AddResponseCaching();
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
                    "image/svg+xml",
                    "image/png",
                    "font/woff",
                    "font/woff2",
                    "font/ttf",
                    "font/eof",
                    "image/x-icon" });
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IProblemService, ProblemService>();
            services.AddScoped<IContestService, ContestService>();
            services.AddScoped<IJudgeService, JudgeService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IVoteService, VoteService>();
            services.AddScoped<IFileService, FileService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ILanguageService, LocalLanguageService>();

            var jss = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            services.AddEFSecondLevelCache();
            services.AddSingleton(typeof(ICacheManagerConfiguration), new CacheManager.Core.ConfigurationBuilder()
                    .WithUpdateMode(CacheUpdateMode.Up)
                    .WithJsonSerializer(jss, jss)
                    .WithMaxRetries(100)
                    .WithRetryTimeout(50)
                    .WithMicrosoftMemoryCacheHandle()
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(3))
                    .Build());

            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));

            services.AddDbContext<WebHostDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
                options.EnableServiceProviderCaching();
            });

            services.AddEntityFrameworkInMemoryDatabase();

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

            services.AddIdentityCore<UserInfo>(options =>
            {
                options.Stores.MaxLengthForKeys = 128;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddSignInManager<SignInManager<UserInfo>>()
            .AddUserManager<CachedUserManager<UserInfo>>()
            .AddEntityFrameworkStores<WebHostDbContext>()
            .AddErrorDescriber<TranslatedIdentityErrorDescriber>()
            .AddDefaultTokenProviders();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<ExceptionMiddleware>();

            services.AddSignalR();

            services.AddMvc()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            })
            /*.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.Converters.Add(new JsonNonStringKeyDictionaryConverterFactory());
            })*/;

            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "wwwroot/dist";
            });
            services.AddReact();

            services.AddJsEngineSwitcher(options =>
            {
                options.DefaultEngineName = ChakraCoreJsEngine.EngineName;
            }).AddChakraCore();

            services.RecordServiceCollection();
            return services.BuildServiceProvider();
        }
    }
}
