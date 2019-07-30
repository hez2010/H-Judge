using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models;
using hjudge.WebHost.Models.Account;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly SignInManager<UserInfo> signInManager;

        public HomeController(CachedUserManager<UserInfo> userManager,
            SignInManager<UserInfo> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AntiForgeryFilter]
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

        [AllowAnonymous]
        public IActionResult Error()
        {
            var ret = new ResultModel
            {
                ErrorCode = (ErrorDescription)HttpContext.Response.StatusCode
            };
            ret.ErrorMessage += $" ({Activity.Current?.Id ?? HttpContext.TraceIdentifier})";
            return new JsonResult(ret);
        }
    }
}
