using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace hjudge.WebHost.Test
{
    public static class UserUtils
    {
        private static bool inited;
        public static async Task InitUsers()
        {
            if (inited) return;
            using var scope = TestService.Scope;
            var userManager = scope.ServiceProvider.GetService<UserManager<UserInfo>>();

            await userManager.CreateAsync(new UserInfo
            {
                UserName = "admin",
                Email = "admin@hjudge.com",
                Name = "admin",
                Privilege = 1
            });
            await userManager.CreateAsync(new UserInfo
            {
                UserName = "teacher",
                Email = "teacher@hjudge.com",
                Name = "teacher",
                Privilege = 2
            });
            await userManager.CreateAsync(new UserInfo
            {
                UserName = "ta",
                Email = "ta@hjudge.com",
                Name = "ta",
                Privilege = 3
            });
            await userManager.CreateAsync(new UserInfo
            {
                UserName = "student",
                Email = "student@hjudge.com",
                Name = "student",
                Privilege = 4
            });
            await userManager.CreateAsync(new UserInfo
            {
                UserName = "black",
                Email = "black@hjudge.com",
                Name = "black",
                Privilege = 5
            });
            inited = true;
        }

        public static async Task<UserInfo> GetAdmin()
        {
            await InitUsers();
            using var scope = TestService.Scope;
            var userManager = scope.ServiceProvider.GetService<UserManager<UserInfo>>();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 1);
        }
        public static async Task<UserInfo> GetTeacher()
        {
            await InitUsers();
            using var scope = TestService.Scope;
            var userManager = scope.ServiceProvider.GetService<UserManager<UserInfo>>();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 2);
        }
        public static async Task<UserInfo> GetTa()
        {
            await InitUsers();
            using var scope = TestService.Scope;
            var userManager = scope.ServiceProvider.GetService<UserManager<UserInfo>>();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 3);
        }
        public static async Task<UserInfo> GetStudent()
        {
            await InitUsers();
            using var scope = TestService.Scope;
            var userManager = scope.ServiceProvider.GetService<UserManager<UserInfo>>();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 4);
        }
        public static async Task<UserInfo> GetBlack()
        {
            await InitUsers();
            using var scope = TestService.Scope;
            var userManager = scope.ServiceProvider.GetService<UserManager<UserInfo>>();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 1);
        }
    }
}
