using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models.Utils;
using hjudge.WebHost.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Controllers
{
    [ApiController]
    [Route("utils")]
    [AutoValidateAntiforgeryToken]
    public class UtilsController : ControllerBase
    {
        private readonly UserManager<UserInfo> userManager;

        public UtilsController(UserManager<UserInfo> userManager)
        {
            this.userManager = userManager;
        }

        /// <summary>
        /// 查询用户，该 API 未启用
        /// </summary>
        /// <param name="patterns">可以根据邮箱、用户名或姓名查询</param>
        /// <returns></returns>
        [Route("queryUsers")]
        [PrivilegeAuthentication.RequireSignedIn]
        [FunctionalControl.DisabledApi]
        [ProducesResponseType(200)]
        public async Task<UserQueryResultListModel> QueryUser(string patterns)
        {
            var user = await userManager.GetUserAsync(User);
            IQueryable<UserInfo> users;
            if (PrivilegeHelper.IsTeacher(user.Privilege))
            {
                users = userManager.Users.Where(i => 
                    (i.Name != null && i.Name.Contains(patterns)) ||
                    i.NormalizedEmail.Contains(patterns.ToUpperInvariant()) ||
                    i.NormalizedUserName.Contains(patterns.ToUpperInvariant()));
            }
            else users = userManager.Users.Where(i => i.NormalizedUserName.Contains(patterns.ToUpper()));

            var result = users.Select(i => new UserQueryResultModel
            {
                UserId = i.Id,
                UserName = i.UserName,
                Name = i.Name,
                Email = i.Email
            });

            return new UserQueryResultListModel
            {
                Users = await result.ToListAsync(),
                TotalCount = await result.CountAsync()
            };
        }
    }
}
