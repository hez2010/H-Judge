using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Middlewares;
using hjudgeWebHost.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace hjudgeWebHostTest
{
    public class MyHttpContextAccessor : IHttpContextAccessor
    {
        public MyHttpContextAccessor()
        {
            HttpContext = new DefaultHttpContext();
        }
        public HttpContext HttpContext { get; set; }
    }
    public class TestService
    {
        public IServiceProvider Provider { get; set; }
        public TestService()
        {
            Provider = InitService();
        }

        private IServiceProvider InitService()
        {
            var services = new ServiceCollection();

            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseSqlite("Data Source=test.db");
            }, ServiceLifetime.Scoped, ServiceLifetime.Singleton);

            services.AddEntityFrameworkSqlite();

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

            services.AddTransient<AntiForgeryFilter>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IProblemService, ProblemService>();
            services.AddTransient<IContestService, ContestService>();

            services.AddIdentityCore<UserInfo>(options =>
            {
                options.Stores.MaxLengthForKeys = 128;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddSignInManager<SignInManager<UserInfo>>()
            .AddUserManager<UserManager<UserInfo>>()
            .AddEntityFrameworkStores<TestDbContext>()
            .AddErrorDescriber<TranslatedIdentityErrorDescriber>();

            services.AddMvc().AddNewtonsoftJson();

            return services.BuildServiceProvider();
        }
    }
}
