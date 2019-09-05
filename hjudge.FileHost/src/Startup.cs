using CacheManager.Core;
using EFSecondLevelCache.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using hjudge.FileHost.Data;
using hjudge.FileHost.Services;

namespace hjudge.FileHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.MaxReceiveMessageSize = 2147483647;
                options.MaxSendMessageSize = 150 * 1048576;
            });

            services.AddDbContext<FileHostDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
// #if DEBUG
//                 options.EnableDetailedErrors(true);
//                 options.EnableSensitiveDataLogging(true);
// #endif
                options.EnableServiceProviderCaching(true);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddEntityFrameworkNpgsql();
            
            services.AddScoped<FileService>();
            services.AddScoped<SeaweedFsService>()
                .Configure<SeaweedFsOptions>(options =>
                {
                    options.MasterHostName = Configuration["SeaweedFs:MasterHostName"];
                    options.Port = int.Parse(Configuration["SeaweedFs:Port"]);
                });

            services.AddEFSecondLevelCache();
            services.AddSingleton(typeof(ICacheManagerConfiguration), new CacheManager.Core.ConfigurationBuilder()
                    .WithUpdateMode(CacheUpdateMode.Up)
                    .WithJsonSerializer()
                    .WithRedisConfiguration(Configuration["Redis:Configuration"], config =>
                    {
                        config.WithAllowAdmin()
                            .WithDatabase(0)
                            .WithEndpoint(Configuration["Redis:HostName"], int.Parse(Configuration["Redis:Port"]));
                    })
                    .WithMaxRetries(100)
                    .WithRetryTimeout(50)
                    .WithRedisCacheHandle(Configuration["Redis:Configuration"])
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
                    .Build());
                    
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Communication with gRPC endpoints must be made through a gRPC client.
                // To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909
                endpoints.MapGrpcService<FileService>();
            });
        }
    }
}
