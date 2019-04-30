﻿using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Middlewares;
using hjudgeWebHost.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using SpanJson.AspNetCore.Formatter;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjudgeWebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
                    "image/svg+xml",
                    "image/png",
                    "font/woff",
                    "font/woff2",
                    "font/ttf",
                    "font/eof" });
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddTransient<AntiForgeryFilter>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IProblemService, ProblemService>();
            services.AddTransient<IContestService, ContestService>();
            services.AddTransient<IJudgeService, JudgeService>();
            services.AddTransient<IGroupService, GroupService>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddSingleton<ILanguageService, LocalLanguageService>();

            var factory = new MessageQueueFactory(
                new MessageQueueFactory.HostOptions
                {
                    HostName = Configuration["MessageQueue:HostName"],
                    Port = int.Parse(Configuration["MessageQueue:Port"]),
                    UserName = Configuration["MessageQueue:UserName"],
                    Password = Configuration["MessageQueue:Password"]
                });

            var cnt = 0;
            while (Configuration.GetSection($"MessageQueue:Producers:{cnt}").Exists())
            {
                factory.CreateProducer(new MessageQueueFactory.ProducerOptions
                {
                    Queue = Configuration[$"MessageQueue:Producers:{cnt}:Queue"],
                    Exchange = Configuration[$"MessageQueue:Producers:{cnt}:Exchange"],
                    RoutingKey = Configuration[$"MessageQueue:Producers:{cnt}:RoutingKey"],
                    Durable = bool.Parse(Configuration[$"MessageQueue:Producers:{cnt}:Durable"]),
                    AutoDelete = bool.Parse(Configuration[$"MessageQueue:Producers:{cnt}:AutoDelete"]),
                    Exclusive = bool.Parse(Configuration[$"MessageQueue:Producers:{cnt}:Exclusive"]),
                });
                ++cnt;
            }

            cnt = 0;
            while (Configuration.GetSection($"MessageQueue:Consumers:{cnt}").Exists())
            {
                factory.CreateConsumer(new MessageQueueFactory.ConsumerOptions
                {
                    Queue = Configuration[$"MessageQueue:Consumers:{cnt}:Queue"],
                    RoutingKey = Configuration[$"MessageQueue:Consumers:{cnt}:RoutingKey"],
                    Durable = bool.Parse(Configuration[$"MessageQueue:Consumers:{cnt}:Durable"]),
                    AutoAck = bool.Parse(Configuration[$"MessageQueue:Consumers:{cnt}:AutoAck"]),
                    Exclusive = bool.Parse(Configuration[$"MessageQueue:Consumers:{cnt}:Exclusive"])
                });
                ++cnt;
            }

            services.AddSingleton<IMessageQueueService, MessageQueueService>()
                .Configure<MessageQueueServiceOptions>(options => options.MessageQueueFactory = factory);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
#if DEBUG
                options.EnableDetailedErrors(true);
                options.EnableSensitiveDataLogging(true);
#endif
                options.EnableServiceProviderCaching(true);
            });

            services.AddEntityFrameworkSqlServer();

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration["Redis:Configuration"];
                options.InstanceName = Configuration["Redis:InstanceName"];
            });

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
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<TranslatedIdentityErrorDescriber>();

            services.AddMvc(options =>
            {
                options.Filters.AddService<AntiForgeryFilter>();
            }).AddSpanJson();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePages(new Func<StatusCodeContext, Task>(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.Body.WriteAsync(
                    Encoding.UTF8.GetBytes($"{{succeeded: false, errorCode: {context.HttpContext.Response.StatusCode}, errorMessage: '请求失败'}}"));
            }));

            app.UseResponseCompression();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
