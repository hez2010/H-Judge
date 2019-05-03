using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Services;
using System.Linq;
using System.Threading.Tasks;

namespace hjudge.WebHost.Test
{
    public class UserUtils
    {
        private static readonly CachedUserManager<UserInfo> userManager = TestService.Provider.GetService(typeof(CachedUserManager<UserInfo>)) as CachedUserManager<UserInfo>;
        private static bool inited;
        public static async Task InitUsers()
        {
            if (inited) return;
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
            return userManager.Users.FirstOrDefault(i => i.Privilege == 1);
        }
        public static async Task<UserInfo> GetTeacher()
        {
            await InitUsers();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 2);
        }
        public static async Task<UserInfo> GetTa()
        {
            await InitUsers();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 3);
        }
        public static async Task<UserInfo> GetStudent()
        {
            await InitUsers();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 4);
        }
        public static async Task<UserInfo> GetBlack()
        {
            await InitUsers();
            return userManager.Users.FirstOrDefault(i => i.Privilege == 1);
        }
    }
}
