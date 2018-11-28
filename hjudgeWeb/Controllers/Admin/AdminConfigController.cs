using hjudgeWeb.Configurations;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    public partial class AdminController : Controller
    {
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
            config.Environments = SystemConfiguration.Config.Environments;
            config.CanDiscussion = SystemConfiguration.Config.CanDiscussion;
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
            public bool CanDiscussion { get; set; }
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

            SystemConfiguration.Config.Environments = submit.Environments;
            SystemConfiguration.Config.CanDiscussion = submit.CanDiscussion;
            Languages.LanguageConfigurations = submit.Languages;

            System.IO.File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "AppData", "SystemConfig.json"), JsonConvert.SerializeObject(SystemConfiguration.Config), Encoding.UTF8);
            System.IO.File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "AppData", "LanguageConfig.json"), JsonConvert.SerializeObject(submit.Languages), Encoding.UTF8);
            return new ResultModel { IsSucceeded = true };
        }
    }
}