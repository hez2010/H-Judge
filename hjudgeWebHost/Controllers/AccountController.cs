using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Middlewares;
using hjudgeWebHost.Models;
using hjudgeWebHost.Models.Account;
using hjudgeWebHost.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly UserManager<UserInfo> UserManager;
        public AccountController(UserManager<UserInfo> userManager)
        {
            UserManager = userManager;
        }

        [HttpPost]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task<ResultModel> UserAvatar(IFormFile avatar)
        {
            var user = await UserManager.GetUserAsync(User);
            var result = new ResultModel { Succeeded = true };

            if (avatar == null)
            {
                result.Succeeded = false;
                result.ErrorMessage = "文件无效";
                result.ErrorCode = 600;
                return result;
            }

            if (!avatar.ContentType.StartsWith("image/"))
            {
                result.Succeeded = false;
                result.ErrorMessage = "只能上传图片文件";
                result.ErrorCode = 600;
                return result;
            }

            if (avatar.Length > 1048576)
            {
                result.Succeeded = false;
                result.ErrorMessage = "图片文件大小不能超过 1 Mb";
                result.ErrorCode = 600;
                return result;
            }

            using var stream = new System.IO.MemoryStream();

            await avatar.CopyToAsync(stream);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);
            user.Avatar = buffer;
            await UserManager.UpdateAsync(user);

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> UserAvatar(string userId)
        {
            var user = await (string.IsNullOrEmpty(userId) ? UserManager.GetUserAsync(User) : UserManager.FindByIdAsync(userId));
            if (user == null)
            {
                return NotFound();
            }
            if (user.Avatar == null || user.Avatar.Length == 0)
            {
                return File(ImageScaler.ScaleImage(Properties.Resource.DefaultAvatar, 128, 128), "image/png");
            }
            return File(ImageScaler.ScaleImage(user.Avatar, 128, 128), "image/png");
        }

        [HttpPost]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task<ResultModel> UserInfo([FromBody]UserInfoModel model)
        {
            var ret = new ResultModel { Succeeded = true };
            var user = await UserManager.GetUserAsync(User);

            user.Name = model.Name;

            if (user.Email != model.Email)
            {
                var result = await UserManager.SetEmailAsync(user, model.Email);
                if (!result.Succeeded)
                {
                    ret.Succeeded = result.Succeeded;
                    ret.ErrorCode = 600;
                    if (result.Errors.Any()) ret.ErrorMessage = result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next);
                    return ret;
                }
            }
            if (user.PhoneNumber != model.PhoneNumber)
            {
                var result = await UserManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!result.Succeeded)
                {
                    ret.Succeeded = result.Succeeded;
                    ret.ErrorCode = 600;
                    if (result.Errors.Any()) ret.ErrorMessage = result.Errors.Select(i => i.Description).Aggregate((accu, next) => accu + "\n" + next);
                    return ret;
                }
            }

            await UserManager.UpdateAsync(user);
            return ret;
        }

        [HttpGet]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task<UserInfoModel> UserInfo(string userId)
        {
            var userInfoRet = new UserInfoModel
            {
                Succeeded = true
            };
            var user = await (string.IsNullOrEmpty(userId) ? UserManager.GetUserAsync(User) : UserManager.FindByIdAsync(userId));
            if (user == null)
            {
                userInfoRet.Succeeded = false;
                userInfoRet.ErrorCode = 404;
                userInfoRet.ErrorMessage = "User not found";
                return userInfoRet;
            }
            userInfoRet.Name = user.Name;
            userInfoRet.UserId = user.Id;
            userInfoRet.UserName = user.UserName;
            userInfoRet.Privilege = user.Privilege;

            if (userInfoRet.SignedIn)
            {
                userInfoRet.EmailConfirmed = user.EmailConfirmed;
                userInfoRet.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userInfoRet.Email = user.Email;
                userInfoRet.PhoneNumber = user.PhoneNumber;
                userInfoRet.OtherInfo = JsonConvert.DeserializeObject<OtherInfo[]>(user.OtherInfo);
            }

            return userInfoRet;
        }
    }
}
