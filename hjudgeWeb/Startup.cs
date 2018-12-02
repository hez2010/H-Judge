using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Hubs;
using hjudgeWeb.Middleware;
using hjudgeWeb.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace hjudgeWeb
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
            //Add brotli compression
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
                    "image/svg+xml",
                    "image/png",
                    "font/woff",
                    "font/woff2",
                    "font/ttf",
                    "font/eof" });
            });

            //EF Core db context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //Identity
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
                .AddIdentityCookies();

            services.AddIdentityCore<UserInfo>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.User.RequireUniqueEmail = true;
            })
            .AddSignInManager()
            .AddUserManager<UserManager<UserInfo>>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<TranslatedIdentityErrorDescriber>();

            //EF Core -- SqlServer
            services.AddEntityFrameworkSqlServer();

            //Email sender transient
            services.AddTransient<IEmailSender, EmailSender>();

            //Register service for anti CSRF attack
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-Token");
            services.AddTransient<AntiForgeryFilter>();

            //Chat hub: signalR
            services.AddSignalR();

            //MVC
            services.AddMvc(options =>
            {
                options.Filters.AddService(typeof(AntiForgeryFilter));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //Brotli compression
            app.UseResponseCompression();

            //Accessibility for ./wwwroot
            app.UseStaticFiles();

            app.UseAuthentication();

            //signalR routing
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/ChatHub");
            });

            //Route and spa fallback
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
