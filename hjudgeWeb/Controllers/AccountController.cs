using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly UserManager<UserInfo> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        //TODO: reimpl IEmailSender
        public readonly IEmailSender _emailSender;

        public AccountController(SignInManager<UserInfo> signInManager,
            UserManager<UserInfo> userManager,
            DbContextOptions<ApplicationDbContext> dbContextOptions,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContextOptions = dbContextOptions;
            _emailSender = emailSender;
        }

        public class ExperienceCoins
        {
            public long Experience { get; set; }
            public long Coins { get; set; }
        }

        [HttpGet]
        public async Task<ExperienceCoins> GetFortune()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                using (var db = new ApplicationDbContext(_dbContextOptions))
                {
                    var fortune = db.Users.Select(i => new { i.Id, i.Experience, i.Coins }).FirstOrDefault(i => i.Id == user.Id);
                    return new ExperienceCoins { Experience = fortune.Experience, Coins = fortune.Coins };
                }
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserAvatar(string userId)
        {
            var user = await (string.IsNullOrEmpty(userId) ? _userManager.GetUserAsync(User) : _userManager.FindByIdAsync(userId));
            if (user == null)
            {
                return null;
            }
            if (user.Avatar == null || user.Avatar.Length == 0)
            {
                return File(Utils.ImageScaler.ScaleImage(Convert.FromBase64String(Properties.Resource.DefaultAvatar), 128, 128), "image/png");
            }
            return File(Utils.ImageScaler.ScaleImage(user.Avatar, 128, 128), "image/png");
        }

        public class EmailModel
        {
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> SendEmailConfirmToken()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var html = $@"<h2>H::Judge</h2>
<p>您好 {user.UserName}，感谢使用 H::Judge！</p>
<p>您请求的验证邮箱地址验证码为：</p><small>{token}</small><p>请使用此验证码验证您的邮箱地址</p>
<hr />
<small>{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}</small>";
                await _emailSender.SendEmailAsync(user.Email, "H::Judge - 验证邮箱地址", html);
            }
            return new ResultModel
            {
                IsSucceeded = true
            };
        }

        public class EmailConfirmModel
        {
            public string Token { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> ConfirmEmail([FromBody]EmailConfirmModel model)
        {
            var ret = new ResultModel { IsSucceeded = true };
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, model.Token);

                if (!result.Succeeded)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = result.Errors.Any() ? result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next) : "注册失败";
                }
                return ret;
            }
            else
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "找不到该用户";
            }
            return ret;
        }

        public class PasswordResetModel
        {
            public string UserName { get; set; }
            public string Token { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> ResetPassword([FromBody]PasswordResetModel model)
        {
            var ret = new ResultModel { IsSucceeded = true };
            if (model.Password != model.ConfirmPassword)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "两次输入的密码不一致";
                return ret;
            }
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "密码重置失败";
                return ret;
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (!result.Succeeded)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = result.Errors.Any() ? result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next) : "注册失败";
            }
            return ret;
        }

        [HttpPost]
        public async Task<ResultModel> SendPasswordResetToken([FromBody]EmailModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && user.Email == model.Email)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var html = $@"<h2>H::Judge</h2>
<p>您好 {user.UserName}，感谢使用 H::Judge！</p>
<p>您请求的重置密码验证码为：</p><small>{token}</small><p>请使用此验证码重置您的密码</p>
<hr />
<small>{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}</small>";
                await _emailSender.SendEmailAsync(user.Email, "H::Judge - 重置密码", html);
            }
            return new ResultModel
            {
                IsSucceeded = true
            };
        }

        [HttpPost]
        public async Task<ResultModel> UpdateAvatar(IFormFile file)
        {
            var result = new ResultModel { IsSucceeded = true };
            if (!_signInManager.IsSignedIn(User))
            {
                result.IsSucceeded = false;
                result.ErrorMessage = "未登录";
                return result;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                result.IsSucceeded = false;
                result.ErrorMessage = "找不到当前用户";
                return result;
            }

            if (file == null)
            {
                result.IsSucceeded = false;
                result.ErrorMessage = "文件无效";
                return result;
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                result.IsSucceeded = false;
                result.ErrorMessage = "只能上传图片文件";
                return result;
            }

            if (file.Length > 1048576)
            {
                result.IsSucceeded = false;
                result.ErrorMessage = "图片文件大小不能超过 1 Mb";
                return result;
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer);
                user.Avatar = buffer;
                await _userManager.UpdateAsync(user);
            }
            return result;
        }

        [HttpGet]
        public async Task<UserInfoModel> GetUserInfo(string userId)
        {
            var userInfo = new UserInfoModel { IsSignedIn = true };
            if (string.IsNullOrEmpty(userId))
            {
                if (!_signInManager.IsSignedIn(User))
                {
                    userInfo.IsSignedIn = false;
                    return userInfo;
                }
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    userInfo.IsSignedIn = false;
                    return userInfo;
                }

                if (user.LastSignedIn.Date < DateTime.Now.Date)
                {
                    userInfo.CoinsBonus = 20;
                    if (user.LastSignedIn.Date + TimeSpan.FromDays(1) >= DateTime.Now.Date)
                    {
                        user.ContinuousSignedIn++;
                    }
                    else
                    {
                        user.ContinuousSignedIn = 0;
                    }
                    user.LastSignedIn = DateTime.Now;
                    userInfo.CoinsBonus += user.ContinuousSignedIn;
                    user.Coins += userInfo.CoinsBonus;
                    await _userManager.UpdateAsync(user);
                }

                userInfo.Id = user.Id;
                userInfo.Email = user.Email;
                userInfo.EmailConfirmed = user.EmailConfirmed;
                userInfo.Experience = user.Experience;
                userInfo.Coins = user.Coins;
                userInfo.PhoneNumber = user.PhoneNumber;
                userInfo.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userInfo.UserName = user.UserName;
                userInfo.OtherInfo = IdentityHelper.GetOtherUserInfo(user.OtherInfo);
                userInfo.Name = user.Name;
                userInfo.Privilege = user.Privilege;
            }
            else
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    userInfo.IsSignedIn = false;
                    return userInfo;
                }

                userInfo.Id = user.Id;
                userInfo.Experience = user.Experience;
                userInfo.Coins = user.Coins;
                userInfo.UserName = user.UserName;
                userInfo.Name = user.Name;
                userInfo.OtherInfo = IdentityHelper.GetOtherUserInfo(user.OtherInfo);
                userInfo.Privilege = user.Privilege;
                userInfo.CoinsBonus = 0;
            }
            return userInfo;
        }

        public class LoginModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool RememberMe { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> Login([FromBody]LoginModel loginInfo)
        {
            await _signInManager.SignOutAsync();
            var result = await _signInManager.PasswordSignInAsync(loginInfo.UserName, loginInfo.Password, loginInfo.RememberMe, false);
            var ret = new ResultModel { IsSucceeded = true };
            if (!result.Succeeded)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "用户名或密码不正确";
            }
            return ret;
        }

        public class RegisterModel
        {
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> Register([FromBody]RegisterModel registerInfo)
        {
            var ret = new ResultModel { IsSucceeded = true };
            if (registerInfo.Password != registerInfo.ConfirmPassword)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "两次输入的密码不一致";
                return ret;
            }
            await _signInManager.SignOutAsync();

            var user = new UserInfo
            {
                UserName = registerInfo.UserName,
                Email = registerInfo.Email,
                Privilege = 4
            };
            var result = await _userManager.CreateAsync(user, registerInfo.Password);

            if (!result.Succeeded)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = result.Errors.Any() ? result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next) : "注册失败";
            }
            else
            {
                await _signInManager.SignInAsync(user, false);
            }
            return ret;
        }

        [HttpPost]
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        [HttpPost]
        public async Task<ResultModel> UpdateOtherInfo([FromBody]OtherUserInfo otherUserInfo)
        {
            var ret = new ResultModel { IsSucceeded = true };
            if (!_signInManager.IsSignedIn(User))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "未登录";
                return ret;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "找不到当前用户";
                return ret;
            }

            if (otherUserInfo != null)
            {
                user.OtherInfo = JsonConvert.SerializeObject(otherUserInfo);
                var result = await _userManager.UpdateAsync(user);
                ret.IsSucceeded = result.Succeeded;
                if (!ret.IsSucceeded)
                {
                    ret.ErrorMessage = result.Errors.Any() ? result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next) : "修改失败";
                }
            }
            return ret;
        }

        public class UpdateInfoModel
        {
            public string Value { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> UpdateName([FromBody]UpdateInfoModel name)
        {
            var ret = new ResultModel();
            if (!_signInManager.IsSignedIn(User))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "未登录";
                return ret;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "找不到当前用户";
                return ret;
            }
            user.Name = name.Value;
            var result = await _userManager.UpdateAsync(user);
            ret.IsSucceeded = result.Succeeded;
            if (!ret.IsSucceeded)
            {
                ret.ErrorMessage = result.Errors.Any() ? result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next) : "修改失败";
            }
            return ret;
        }

        [HttpPost]
        public async Task<ResultModel> UpdateEmail([FromBody]UpdateInfoModel email)
        {
            var ret = new ResultModel();
            if (!_signInManager.IsSignedIn(User))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "未登录";
                return ret;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "找不到当前用户";
                return ret;
            }
            var result = await _userManager.SetEmailAsync(user, email.Value);
            ret.IsSucceeded = result.Succeeded;
            if (!ret.IsSucceeded)
            {
                ret.ErrorMessage = result.Errors.Any() ? result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next) : "修改失败";
            }
            return ret;
        }

        [HttpPost]
        public async Task<ResultModel> UpdatePhoneNumber([FromBody]UpdateInfoModel phoneNumber)
        {
            var ret = new ResultModel();
            if (!_signInManager.IsSignedIn(User))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "未登录";
                return ret;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "找不到当前用户";
                return ret;
            }
            var result = await _userManager.SetPhoneNumberAsync(user, phoneNumber.Value);

            ret.IsSucceeded = result.Succeeded;
            if (!ret.IsSucceeded)
            {
                ret.ErrorMessage = result.Errors.Any() ? result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next) : "修改失败";
            }
            return ret;
        }
    }
}