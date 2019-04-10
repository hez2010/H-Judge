using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHostTest
{
    public class UserUtils
    {
        private static readonly CachedUserManager<UserInfo> userManager = TestService.Provider.GetService(typeof(CachedUserManager<UserInfo>)) as CachedUserManager<UserInfo>;
        public static async Task InitUsers()
        {
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
        }

        public static UserInfo GetAdmin()
        {
            return userManager.Users.FirstOrDefault(i => i.Privilege == 1);
        }
        public static UserInfo GetTeacher()
        {
            return userManager.Users.FirstOrDefault(i => i.Privilege == 2);
        }
        public static UserInfo GetTa()
        {
            return userManager.Users.FirstOrDefault(i => i.Privilege == 3);
        }
        public static UserInfo GetStudent()
        {
            return userManager.Users.FirstOrDefault(i => i.Privilege == 4);
        }
        public static UserInfo GetBlack()
        {
            return userManager.Users.FirstOrDefault(i => i.Privilege == 1);
        }
    }
}
