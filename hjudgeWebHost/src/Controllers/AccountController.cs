using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Middlewares;
using hjudgeWebHost.Models;
using hjudgeWebHost.Models.Account;
using hjudgeWebHost.Services;
using hjudgeWebHost.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly SignInManager<UserInfo> signInManager;
        private readonly ApplicationDbContext dbContext;

        public AccountController(CachedUserManager<UserInfo> userManager, SignInManager<UserInfo> signInManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbContext = dbContext;
        }

        public class LoginModel
        {
            [Required]
            public string UserName { get; set; } = string.Empty;
            [Required]
            public string Password { get; set; } = string.Empty;
            public bool RememberMe { get; set; }
        }
        [HttpPost]
        public async Task<ResultModel> Login([FromBody]LoginModel model)
        {
            var ret = new ResultModel();
            if (TryValidateModel(model))
            {
                await signInManager.SignOutAsync();
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (!result.Succeeded)
                {
                    ret.ErrorCode = ErrorDescription.AuthenticationFailed;
                }
            }
            else
            {
                ret.ErrorCode = ErrorDescription.ArgumentError;
            }
            return ret;
        }

        public class RegisterModel
        {
            [Required]
            public string UserName { get; set; } = string.Empty;
            [Required]
            public string Name { get; set; } = string.Empty;
            [Required]
            public string Password { get; set; } = string.Empty;
            [Required]
            public string ConfirmPassword { get; set; } = string.Empty;
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<ResultModel> Register([FromBody]RegisterModel model)
        {
            var ret = new ResultModel();
            if (TryValidateModel(model))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ret.ErrorCode = ErrorDescription.ArgumentError;
                    ret.ErrorMessage = "两次输入的密码不一致";
                    return ret;
                }
                await signInManager.SignOutAsync();
                var user = new UserInfo
                {
                    UserName = model.UserName,
                    Name = model.Name,
                    Email = model.Email,
                    Privilege = 4
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    ret.ErrorCode = ErrorDescription.ArgumentError;
                    if (result.Errors.Any()) ret.ErrorMessage = result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next);
                }
                else await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
            }
            else
            {
                ret.ErrorCode = ErrorDescription.ArgumentError;
            }
            return ret;
        }

        [HttpPost]
        public async Task<ResultModel> Logout()
        {
            await signInManager.SignOutAsync();
            return new ResultModel();
        }

        [HttpPost]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task<ResultModel> UserAvatar(IFormFile avatar)
        {
            var user = await userManager.GetUserAsync(User);
            var result = new ResultModel();

            if (avatar == null)
            {
                result.ErrorCode = ErrorDescription.FileBadFormat;
                return result;
            }

            if (!avatar.ContentType.StartsWith("image/"))
            {
                result.ErrorCode = ErrorDescription.FileBadFormat;
                return result;
            }

            if (avatar.Length > 1048576)
            {
                result.ErrorCode = ErrorDescription.FileSizeExceeded;
                return result;
            }

            using var stream = new System.IO.MemoryStream();

            await avatar.CopyToAsync(stream);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);
            user.Avatar = buffer;
            await userManager.UpdateAsync(user);

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> UserAvatar(string? userId)
        {
            var user = await (string.IsNullOrEmpty(userId) ? userManager.GetUserAsync(User) : userManager.FindByIdAsync(userId));
            if (user == null)
            {
                return NotFound();
            }
            if (user.Avatar.Length == 0)
            {
                return File(ImageScaler.ScaleImage(Properties.Resource.DefaultAvatar, 128, 128), "image/png");
            }
            return File(ImageScaler.ScaleImage(user.Avatar, 128, 128), "image/png");
        }

        [HttpPost]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task<ResultModel> UserInfo([FromBody]UserInfoModel model)
        {
            var ret = new ResultModel();
            var user = await userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(model.Name)) user.Name = model.Name;

            if (!string.IsNullOrEmpty(model.Email) && user.Email != model.Email)
            {
                var result = await userManager.SetEmailAsync(user, model.Email);
                if (!result.Succeeded)
                {
                    ret.ErrorCode = ErrorDescription.ArgumentError;
                    if (result.Errors.Any()) ret.ErrorMessage = result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next);
                    return ret;
                }
            }
            if (model.PhoneNumber != null && user.PhoneNumber != model.PhoneNumber)
            {
                var result = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!result.Succeeded)
                {
                    ret.ErrorCode = ErrorDescription.ArgumentError;
                    if (result.Errors.Any()) ret.ErrorMessage = result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next);
                    return ret;
                }
            }
            if (model.OtherInfo != null)
            {
               var otherInfoJson = (string.IsNullOrEmpty(user.OtherInfo) ? "{}" : user.OtherInfo).DeserializeJson<IDictionary<string, string>>();
                foreach (var info in model.OtherInfo)
                {
                    if (IdentityHelper.OtherInfoProperties.Any(i => i.Name == info.Key))
                    {
                        otherInfoJson[info.Key] = info.Value;
                    }
                }
                user.OtherInfo = otherInfoJson.SerializeJsonAsString();
            }

            await userManager.UpdateAsync(user);
            return ret;
        }

        [HttpGet]
        public async Task<UserInfoModel> UserInfo(string? userId)
        {
            var userInfoRet = new UserInfoModel
            {
                SignedIn = signInManager.IsSignedIn(User)
            };
            if (string.IsNullOrEmpty(userId)) userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                if (!string.IsNullOrEmpty(userId)) userInfoRet.ErrorCode = ErrorDescription.UserNotExist;
                return userInfoRet;
            }
            userInfoRet.Name = user.Name;
            userInfoRet.UserId = user.Id;
            userInfoRet.UserName = user.UserName;
            userInfoRet.Privilege = user.Privilege;
            userInfoRet.Coins = user.Coins;
            userInfoRet.Experience = user.Experience;

            if (userInfoRet.SignedIn)
            {
                userInfoRet.EmailConfirmed = user.EmailConfirmed;
                userInfoRet.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userInfoRet.Email = user.Email;
                userInfoRet.PhoneNumber = user.PhoneNumber;
                userInfoRet.OtherInfo = IdentityHelper.GetOtherUserInfo(string.IsNullOrEmpty(user.OtherInfo) ? "{}" : user.OtherInfo);
            }

            return userInfoRet;
        }
    }
}
