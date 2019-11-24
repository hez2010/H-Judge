using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using hjudge.Core;
using hjudge.Shared.Utils;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models.Account;
using hjudge.WebHost.Properties;
using hjudge.WebHost.Services;
using hjudge.WebHost.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("user")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserInfo> userManager;
        private readonly SignInManager<UserInfo> signInManager;
        private readonly IJudgeService judgeService;
        private readonly IEmailSender emailSender;

        public AccountController(
            UserManager<UserInfo> userManager,
            SignInManager<UserInfo> signInManager,
            IJudgeService judgeService,
            WebHostDbContext dbContext,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.judgeService = judgeService;
            this.emailSender = emailSender;
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [HttpPost]
        [Route("login")]
        public async Task Login([FromBody]LoginModel model)
        {
            if (TryValidateModel(model))
            {
                await signInManager.SignOutAsync();
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (!result.Succeeded)
                {
                    throw new AuthenticationException("用户名或密码不正确");
                }
            }
            else throw new BadRequestException();
        }

        [HttpPut]
        [Route("register")]
        public async Task Register([FromBody]RegisterModel model)
        {
            if (TryValidateModel(model))
            {
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
                    throw result.Errors.Any() ?
                        new BadRequestException(result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next))
                        : new BadRequestException();
                }

                await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
            }
            else
            {
                var errors = ModelState.SelectMany(i => i.Value.Errors, (i, j) => j.ErrorMessage).ToList();
                throw errors.Any() ?
                    new BadRequestException(errors.Aggregate((accu, next) => accu + "\n" + next))
                    : new BadRequestException();
            }
        }

        [HttpPost]
        [Route("logout")]
        public Task Logout()
        {
            return signInManager.SignOutAsync();
        }

        [HttpPut]
        [Route("avatar")]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task UserAvatar(IFormFile avatar)
        {
            var userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);

            if (avatar == null) throw new BadRequestException("文件格式不正确");

            if (!avatar.ContentType.StartsWith("image/")) throw new BadRequestException("文件格式不正确");

            if (avatar.Length > 1048576) throw new BadRequestException("文件大小不能超过 1 Mb");

            await using var stream = new MemoryStream();

            await avatar.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);
            user.Avatar = buffer;
            await userManager.UpdateAsync(user);
        }

        [HttpGet]
        [Route("avatar")]
        [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "userId" })]
        public async Task<IActionResult> UserAvatar(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            var user = await userManager.FindByIdAsync(userId);
            return user?.Avatar == null || user.Avatar.Length == 0
                ? File(ImageScaler.ScaleImage(Resource.DefaultAvatar, 128, 128), "image/png")
                : File(ImageScaler.ScaleImage(user.Avatar, 128, 128), "image/png");
        }

        [HttpPost]
        [Route("profiles")]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task UserInfo([FromBody]UserInfoModel model)
        {
            var userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);

            if (!string.IsNullOrEmpty(model.Name)) user.Name = model.Name;

            if (!string.IsNullOrEmpty(model.Email) && user.Email != model.Email)
            {
                var result = await userManager.SetEmailAsync(user, model.Email);
                if (!result.Succeeded)
                {
                    throw result.Errors.Any() ?
                        new BadRequestException(result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next))
                        : new BadRequestException();
                }
            }
            if (model.PhoneNumber != null && user.PhoneNumber != model.PhoneNumber)
            {
                var result = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!result.Succeeded)
                {
                    throw result.Errors.Any() ?
                        new BadRequestException(result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next))
                        : new BadRequestException();
                }
            }
            if (model.OtherInfo != null)
            {
                var otherInfoJson = (string.IsNullOrEmpty(user.OtherInfo) ? "{}" : user.OtherInfo).DeserializeJson<IDictionary<string, object?>>();
                foreach (var info in model.OtherInfo)
                {
                    var prop = IdentityHelper.OtherInfoProperties.FirstOrDefault(i => i.Name == info.Key);
                    if (prop != null)
                    {
                        try
                        {
                            otherInfoJson[info.Key] = Convert.ChangeType(info.Value, prop.PropertyType);
                        }
                        catch
                        {
                            throw new BadRequestException("信息填写不正确");
                        }
                    }
                }
                user.OtherInfo = otherInfoJson.SerializeJsonAsString();
            }

            await userManager.UpdateAsync(user);
        }

        [HttpGet]
        [SendAntiForgeryToken]
        [Route("profiles")]
        public async Task<UserInfoModel> UserInfo(string? userId = null)
        {
            var signedIn = signInManager.IsSignedIn(User);
            var userInfoRet = new UserInfoModel
            {
                SignedIn = string.IsNullOrEmpty(userId) && signedIn
            };
            if (string.IsNullOrEmpty(userId)) userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);
            var currentUser = string.IsNullOrEmpty(userId) ? user : await userManager.GetUserAsync(User);
            if (userId == null || user == null) return new UserInfoModel();
            userInfoRet.UserId = user.Id;
            userInfoRet.UserName = user.UserName;
            userInfoRet.Privilege = user.Privilege;
            userInfoRet.Coins = user.Coins;
            userInfoRet.Experience = user.Experience;
            userInfoRet.OtherInfo = IdentityHelper.GetOtherUserInfo(string.IsNullOrEmpty(user.OtherInfo) ? "{}" : user.OtherInfo);

            if (userInfoRet.SignedIn || PrivilegeHelper.IsTeacher(currentUser?.Privilege))
            {
                userInfoRet.Name = user.Name;
                userInfoRet.EmailConfirmed = user.EmailConfirmed;
                userInfoRet.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userInfoRet.Email = user.Email;
                userInfoRet.PhoneNumber = user.PhoneNumber;
            }

            return userInfoRet;
        }

        [HttpGet]
        [Route("stats")]
        public async Task<ProblemStatisticsModel> GetUserProblemStatistics(string? userId = null)
        {
            var ret = new ProblemStatisticsModel();
            if (string.IsNullOrEmpty(userId)) userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);
            if (userId == null || user == null) return new ProblemStatisticsModel();

            var judges = await judgeService.QueryJudgesAsync(userId);

            ret.SolvedProblems = await judges.Where(i => i.ResultType == (int)ResultCode.Accepted).Select(i => i.ProblemId).Distinct().OrderBy(i => i).ToListAsync();
            ret.TriedProblems = await judges.Where(i => i.ResultType != (int)ResultCode.Accepted).Select(i => i.ProblemId).Distinct().OrderBy(i => i).ToListAsync();

            ret.TriedProblems = ret.TriedProblems.Where(i => !ret.SolvedProblems.Contains(i)).ToList();
            return ret;
        }

        [HttpPost]
        [Route("reset-email")]
        public async Task SendResetPasswordEmailAsync([FromBody]ResetEmailModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return;
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var html = $@"<h2>H::Judge</h2>
<p>您好 {user.UserName}，感谢使用 H::Judge！</p>
<p>您请求的重置密码验证码为：</p><small>{token}</small><p>请使用此验证码重置您的密码</p>
<hr />
<small>{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}</small>";
            await emailSender.SendAsync(
                 "重置密码 - H::Judge",
                 html,
                 EmailType.Account,
                 new[] { model.Email });
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task ResetPasswordAsync([FromBody]ResetModel model)
        {
            if (TryValidateModel(model))
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) return;
                var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (!result.Succeeded)
                {
                    throw result.Errors.Any() ?
                        new BadRequestException(result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next))
                        : new BadRequestException();
                }
            }
            else
            {
                var errors = ModelState.SelectMany(i => i.Value.Errors, (i, j) => j.ErrorMessage).ToList();
                throw errors.Any() ?
                    new BadRequestException(errors.Aggregate((accu, next) => accu + "\n" + next))
                    : new BadRequestException();
            }
        }
    }
}
