using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly UserManager<UserInfo> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public AccountController(SignInManager<UserInfo> signInManager,
            UserManager<UserInfo> userManager,
            DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContextOptions = dbContextOptions;
        }

        [HttpGet]
        public async Task<string> GetUserAvatar(string userId)
        {
            var user = await (string.IsNullOrEmpty(userId) ? _userManager.GetUserAsync(User) : _userManager.FindByIdAsync(userId));
            if (user == null)
            {
                return null;
            }

            return Convert.ToBase64String(user.Avatar, Base64FormattingOptions.None);
        }

        [HttpGet]
        public async Task<UserInfoModel> GetUserInfo()
        {
            var userInfo = new UserInfoModel { IsSignedIn = true };
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
            return userInfo;
        }
    }
}