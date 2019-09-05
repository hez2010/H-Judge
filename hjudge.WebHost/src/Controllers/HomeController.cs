using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models.Account;
using hjudge.WebHost.Models.Home;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
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
            if (user == null) return new UserInfoModel();
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

        [HttpGet]
        [Route("home/activities")]
        public ActivityListModel GetActivities()
        {
            // TODO: Generate activities
            return new ActivityListModel();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            throw new InterfaceException((HttpStatusCode)HttpContext.Response.StatusCode, Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        }
    }
}
