using hjudge.Shared.Utils;
using hjudge.WebHost.Utils;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Middlewares;
using EFSecondLevelCache.Core;
using hjudge.WebHost.Models.Account;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using hjudge.Core;
using hjudge.WebHost.Exceptions;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("user")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly SignInManager<UserInfo> signInManager;
        private readonly IJudgeService judgeService;

        public AccountController(CachedUserManager<UserInfo> userManager, SignInManager<UserInfo> signInManager, IJudgeService judgeService, WebHostDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.judgeService = judgeService;
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
                else await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
            }
            else
            {
                var errors = ModelState.ToList().SelectMany(i => i.Value.Errors, (i, j) => j.ErrorMessage);
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

            using var stream = new System.IO.MemoryStream();

            await avatar.CopyToAsync(stream);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);
            user.Avatar = buffer;
            await userManager.UpdateAsync(user);
        }

        [HttpGet]
        [Route("avatar")]
        public async Task<IActionResult> UserAvatar(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);
            return user?.Avatar == null || user.Avatar.Length == 0
                ? File(ImageScaler.ScaleImage(Properties.Resource.DefaultAvatar, 128, 128), "image/png")
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
            var userInfoRet = new UserInfoModel
            {
                SignedIn = signInManager.IsSignedIn(User)
            };
            if (string.IsNullOrEmpty(userId)) userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);
            if (userId == null || user == null) return new UserInfoModel();
            userInfoRet.Name = user.Name;
            userInfoRet.UserId = user.Id;
            userInfoRet.UserName = user.UserName;
            userInfoRet.Privilege = user.Privilege;
            userInfoRet.Coins = user.Coins;
            userInfoRet.Experience = user.Experience;
            userInfoRet.OtherInfo = IdentityHelper.GetOtherUserInfo(string.IsNullOrEmpty(user.OtherInfo) ? "{}" : user.OtherInfo);

            if (userInfoRet.SignedIn)
            {
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

            ret.SolvedProblems = await judges.Where(i => i.ResultType == (int)ResultCode.Accepted).Select(i => i.ProblemId).Distinct().OrderBy(i => i).Cacheable().ToListAsync();
            ret.TriedProblems = await judges.Where(i => i.ResultType != (int)ResultCode.Accepted).Select(i => i.ProblemId).Distinct().OrderBy(i => i).Cacheable().ToListAsync();

            return ret;
        }
    }
}
