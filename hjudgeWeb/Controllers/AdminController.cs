using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly UserManager<UserInfo> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public AdminController(SignInManager<UserInfo> signInManager,
            UserManager<UserInfo> userManager,
            DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContextOptions = dbContextOptions;
        }

        private async Task<(UserInfo, int)> GetUserPrivilegeAsync()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return (null, 0);
            }
            var user = await _userManager.GetUserAsync(User);
            return (user, user?.Privilege ?? 0);
        }

        private bool HasAdminPrivilege(int privilege)
        {
            return privilege == 1;
        }

        [HttpGet]
        public async Task<SystemConfigModel> GetSystemConfig()
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var config = new SystemConfigModel { IsSucceeded = true };
            if (!HasAdminPrivilege(privilege))
            {
                config.IsSucceeded = false;
                config.ErrorMessage = "没有权限";
                return config;
            }
            config.System = RuntimeInformation.OSDescription + " " + RuntimeInformation.OSArchitecture;
            config.Environments = SystemConfiguration.Environments;
            config.Languages = Languages.LanguageConfigurations;
            return config;
        }

        public class SystemConfigSubmitModel
        {
            public SystemConfigSubmitModel()
            {
                Languages = new List<LanguageConfiguration>();
            }

            public string Environments { get; set; }
            public List<LanguageConfiguration> Languages { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> UpdateSystemConfig([FromBody]SystemConfigSubmitModel submit)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            if (!HasAdminPrivilege(privilege))
            {
                return new ResultModel { IsSucceeded = false, ErrorMessage = "没有权限" };
            }

            SystemConfiguration.Environments = submit.Environments;
            Languages.LanguageConfigurations = submit.Languages;

            System.IO.File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "AppData", "SystemConfig.json"), JsonConvert.SerializeObject(new { submit.Environments }), Encoding.UTF8);
            System.IO.File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "AppData", "LanguageConfig.json"), JsonConvert.SerializeObject(submit.Languages), Encoding.UTF8);
            return new ResultModel { IsSucceeded = true };
        }
    }
}