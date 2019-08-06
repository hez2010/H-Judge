using hjudge.Shared.MessageQueue;
using hjudge.Shared.Utils;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Extensions;
using hjudge.WebHost.MessageHandlers;
using hjudge.WebHost.Services;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using React.AspNet;
using System;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using CacheManager.Core;
using hjudge.Shared.Caching;
using hjudge.WebHost.Models;

namespace hjudge.WebHost
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
                    "font/eof" });
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IProblemService, ProblemService>();
            services.AddScoped<IContestService, ContestService>();
            services.AddScoped<IJudgeService, JudgeService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IVoteService, VoteService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ILanguageService, LocalLanguageService>();

            services.AddMessageHandlers();

            services.AddSingleton<IMessageQueueService, MessageQueueService>()
                .Configure<MessageQueueServiceOptions>(options => options.MessageQueueFactory = CreateMessageQueueInstance());

            services.AddEFSecondLevelCache();
            services.AddSingleton(typeof(ICacheManagerConfiguration), new CacheManager.Core.ConfigurationBuilder()
                    .WithUpdateMode(CacheUpdateMode.Up)
                    .WithSerializer(typeof(CacheItemJsonSerializer))
                    .WithRedisConfiguration(configuration["Redis:Configuration"], config =>
                    {
                        config.WithAllowAdmin()
                            .WithDatabase(0)
                            .WithEndpoint(configuration["Redis:HostName"], int.Parse(configuration["Redis:Port"]));
                    })
                    .WithMaxRetries(100)
                    .WithRetryTimeout(50)
                    .WithRedisCacheHandle(configuration["Redis:Configuration"])
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
                    .Build());

            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));

            services.AddDbContext<WebHostDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(true);
                }
                options.EnableServiceProviderCaching(true);
            });

            services.AddEntityFrameworkNpgsql();

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
            .AddErrorDescriber<TranslatedIdentityErrorDescriber>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc();

            if (environment.IsProduction())
            {
                services.AddReact();

                services.AddJsEngineSwitcher(options =>
                {
                    options.DefaultEngineName = ChakraCoreJsEngine.EngineName;
                }).AddChakraCore();

                services.AddSpaStaticFiles(options =>
                {
                    options.RootPath = "wwwroot/dist";
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            if (lifetime.ApplicationStopped.IsCancellationRequested)
            {
                var mqService = MessageHandlersServiceExtensions.ServiceProvider.GetService<IMessageQueueService>();
                mqService?.Dispose();
            }

            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(config =>
                {
                    config.Run(async context =>
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.Body.WriteAsync(new ResultModel
                        {
                            Succeeded = false,
                            ErrorCode = ErrorDescription.InternalServerException
                        }.SerializeJson(true));
                    });
                });
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePages(new Func<StatusCodeContext, Task>(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.Body.WriteAsync(
                    new ResultModel
                    {
                        Succeeded = false,
                        ErrorCode = (ErrorDescription)context.HttpContext.Response.StatusCode,
                        ErrorMessage = "请求失败"
                    }.SerializeJson(true));
            }));

            app.UseResponseCaching();
            app.UseResponseCompression();

            if (environment.IsProduction())
            {
                app.UseReact(config =>
                {
                    config.UseServerSideRendering = true;
                    config.SetReuseJavaScriptEngines(true);
                    config.SetUseDebugReact(environment.IsDevelopment());
                    config.SetAllowJavaScriptPrecompilation(true);
                    config.AddScriptWithoutTransform("~/dist/*.js");
                });
            }

            app.UseStaticFiles();
            if (environment.IsProduction())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                if (environment.IsProduction())
                {
                    endpoints.MapControllerRoute(
                        name: "frontend",
                        pattern: "{path?}",
                        defaults: new { Controller = "Home", Action = "Index" });
                    endpoints.MapControllerRoute(
                        name: "frontend-params",
                        pattern: "{path?}/{**params}",
                        defaults: new { Controller = "Home", Action = "Index" });

                    endpoints.MapRazorPages();
                }
            });

            if (environment.IsDevelopment())
            {
                app.UseSpa(options =>
                {
                    options.Options.DefaultPage = "/";
                    options.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                });
            }
        }

        public MessageQueueFactory CreateMessageQueueInstance()
        {
            var factory = new MessageQueueFactory(
                new MessageQueueFactory.HostOptions
                {
                    HostName = configuration["MessageQueue:HostName"],
                    VirtualHost = configuration["MessageQueue:VirtualHost"],
                    Port = int.Parse(configuration["MessageQueue:Port"]),
                    UserName = configuration["MessageQueue:UserName"],
                    Password = configuration["MessageQueue:Password"]
                });

            var cnt = -1;
            while (configuration.GetSection($"MessageQueue:Producers:{++cnt}").Exists())
            {
                factory.CreateProducer(new MessageQueueFactory.ProducerOptions
                {
                    Queue = configuration[$"MessageQueue:Producers:{cnt}:Queue"],
                    Durable = bool.Parse(configuration[$"MessageQueue:Producers:{cnt}:Durable"]),
                    AutoDelete = bool.Parse(configuration[$"MessageQueue:Producers:{cnt}:AutoDelete"]),
                    Exclusive = bool.Parse(configuration[$"MessageQueue:Producers:{cnt}:Exclusive"]),
                    Exchange = configuration[$"MessageQueue:Producers:{cnt}:Exchange"],
                    RoutingKey = configuration[$"MessageQueue:Producers:{cnt}:RoutingKey"]
                });
            }

            cnt = -1;
            while (configuration.GetSection($"MessageQueue:Consumers:{++cnt}").Exists())
            {
                factory.CreateConsumer(new MessageQueueFactory.ConsumerOptions
                {
                    Queue = configuration[$"MessageQueue:Consumers:{cnt}:Queue"],
                    Durable = bool.Parse(configuration[$"MessageQueue:Consumers:{cnt}:Durable"]),
                    AutoAck = bool.Parse(configuration[$"MessageQueue:Consumers:{cnt}:AutoAck"]),
                    Exclusive = bool.Parse(configuration[$"MessageQueue:Consumers:{cnt}:Exclusive"]),
                    Exchange = configuration[$"MessageQueue:Producers:{cnt}:Exchange"],
                    RoutingKey = configuration[$"MessageQueue:Producers:{cnt}:RoutingKey"],
                    OnReceived = configuration[$"MessageQueue:Consumers:{cnt}:Queue"] switch
                    {
                        "JudgeReport" => new AsyncEventHandler<BasicDeliverEventArgs>(JudgeReport.JudgeReport_Received),
                        _ => null
                    }
                });
            }

            return factory;
        }
    }
}
