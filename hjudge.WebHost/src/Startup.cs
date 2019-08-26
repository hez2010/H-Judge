using hjudge.Shared.MessageQueue;
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
using hjudge.WebHost.Models;
using hjudge.WebHost.Middlewares;
using Newtonsoft.Json;
using hjudge.WebHost.Hubs;
using System.Threading;

namespace hjudge.WebHost
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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

            services.AddSingleton<IMessageQueueService, MessageQueueService>()
                .Configure<MessageQueueServiceOptions>(options => options.MessageQueueFactory = CreateMessageQueueInstance());

            var jss = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            services.AddEFSecondLevelCache();
            services.AddSingleton(typeof(ICacheManagerConfiguration), new CacheManager.Core.ConfigurationBuilder()
                    .WithUpdateMode(CacheUpdateMode.Up)
                    .WithJsonSerializer(jss, jss)
                    .WithRedisConfiguration(configuration["Redis:Configuration"], config =>
                    {
                        config.WithAllowAdmin()
                            .WithDatabase(0)
                            .WithEndpoint(configuration["Redis:HostName"], int.Parse(configuration["Redis:Port"]));
                    })
                    .WithMaxRetries(100)
                    .WithRetryTimeout(50)
                    .WithRedisCacheHandle(configuration["Redis:Configuration"])
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(3))
                    .Build());

            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));

            services.AddDbContext<WebHostDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
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

            if (environment.IsProduction())
            {
                services.AddReact();

                services.AddJsEngineSwitcher(options =>
                {
                    options.DefaultEngineName = ChakraCoreJsEngine.EngineName;
                }).AddChakraCore();
            }
            
            services.RecordServiceCollection();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            if (lifetime.ApplicationStopped.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                var mqService = Program.RootServiceProvider?.GetService<IMessageQueueService>();
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
                    config.Run(context => ExceptionMiddleware.WriteExceptionAsync(context, new ErrorModel
                    {
                        ErrorCode = System.Net.HttpStatusCode.InternalServerError,
                        ErrorMessage = "服务器内部异常"
                    }));
                });
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<ExceptionMiddleware>(); // must be placed after app.UseExceptionHandler()

            app.UseStatusCodePages(new Func<StatusCodeContext, Task>(context =>
                ExceptionMiddleware.WriteExceptionAsync(context.HttpContext, new ErrorModel
                {
                    ErrorCode = (System.Net.HttpStatusCode)context.HttpContext.Response.StatusCode,
                    ErrorMessage = "请求失败"
                })));

            app.UseResponseCaching();
            app.UseResponseCompression();

            if (environment.IsProduction())
            {
                app.UseReact(config =>
                {
                    config.UseServerSideRendering = true;
                    config.LoadBabel = false;
                    config.LoadReact = false;
                    config.ExceptionHandler = (ex, arg1, arg2) =>
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine(arg1);
                        Console.WriteLine(arg2);
                    };
                    config.SetReuseJavaScriptEngines(true);
                    config.SetAllowJavaScriptPrecompilation(true);
                    config.AddScriptWithoutTransform("~/dist/*.js");
                });
            }

            app.UseSpaStaticFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();

                endpoints.MapHub<JudgeHub>("/hub/judge");

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
                app.UseSpa(options => options.UseProxyToSpaDevelopmentServer("http://localhost:3000/"));
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
                var consumer = new MessageQueueFactory.ConsumerOptions
                {
                    Queue = configuration[$"MessageQueue:Consumers:{cnt}:Queue"],
                    Durable = bool.Parse(configuration[$"MessageQueue:Consumers:{cnt}:Durable"]),
                    AutoAck = bool.Parse(configuration[$"MessageQueue:Consumers:{cnt}:AutoAck"]),
                    Exclusive = bool.Parse(configuration[$"MessageQueue:Consumers:{cnt}:Exclusive"]),
                    Exchange = configuration[$"MessageQueue:Producers:{cnt}:Exchange"],
                    RoutingKey = configuration[$"MessageQueue:Producers:{cnt}:RoutingKey"]
                };
                switch (configuration[$"MessageQueue:Consumers:{cnt}:Queue"])
                {
                    case "JudgeReport":
                        consumer.OnReceived = new AsyncEventHandler<BasicDeliverEventArgs>(JudgeReport.JudgeReport_Received);
                        Task.Run(() => JudgeReport.QueueExecutor(cancellationTokenSource.Token));
                        break;
                }
                factory.CreateConsumer(consumer);
            }

            return factory;
        }
    }
}
