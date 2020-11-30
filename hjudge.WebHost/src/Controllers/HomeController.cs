﻿using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using hjudge.Core;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models.Account;
using hjudge.WebHost.Models.Home;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly UserManager<UserInfo> userManager;
        private readonly SignInManager<UserInfo> signInManager;
        private readonly IJudgeService judgeService;

        public HomeController(UserManager<UserInfo> userManager,
            SignInManager<UserInfo> signInManager,
            IJudgeService judgeService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.judgeService = judgeService;
        }

        [SendAntiForgeryToken]
        public async Task<IActionResult> Index()
        {
            var model = await GetCurrentUserInfo();
            return View(model);
        }

        private async Task<UserInfoModel> GetCurrentUserInfo()
        {
            var userInfoRet = new UserInfoModel
            {
                SignedIn = signInManager.IsSignedIn(User)
            };
            var userId = userManager.GetUserId(User);
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return new UserInfoModel();
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
                userInfoRet.OtherInfo =
                    IdentityHelper.GetOtherUserInfo(string.IsNullOrEmpty(user.OtherInfo) ? "{}" : user.OtherInfo);
            }

            return userInfoRet;
        }

        /// <summary>
        /// 获取首页动态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("home/activities")]
        [ProducesResponseType(200)]
        public async Task<ActivityListModel> GetActivities()
        {
            var judges = await judgeService
                .QueryJudgesAsync(null, 0, 0, 0, (int)ResultCode.Accepted);
            var result = judges
                .Where(i => (i.ContestId == null && i.GroupId == null && !i.Problem.Hidden) ||
                    (i.ContestId != null && i.GroupId == null && !i.Contest.Hidden) ||
                    (i.GroupId != null && !i.Group.IsPrivate))
                .Select(i =>
                new
                {
                    ProblemName = i.Problem.Name,
                    i.UserInfo.UserName,
                    i.UserId,
                    Time = i.JudgeTime,
                    Title = "通过题目",
                    Content = $"成功通过了题目 {i.Problem.Name}"
                })
                .OrderByDescending(i => i.Time);

            var model = new ActivityListModel
            {
                TotalCount = 10
            };

            foreach (var i in result.Take(10))
            {
                model.Activities.Add(new ActivityModel
                {
                    Content = $"成功通过了题目 {i.ProblemName}",
                    Time = i.Time,
                    Title = "通过题目",
                    UserId = i.UserId,
                    UserName = i.UserName
                });
            }

            return model;
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            throw new InterfaceException((HttpStatusCode)HttpContext.Response.StatusCode,
                Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        }
    }
}