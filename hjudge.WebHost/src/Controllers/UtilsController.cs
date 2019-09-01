using EFSecondLevelCache.Core;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models.Utils;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace hjudge.WebHost.Controllers
{
    [ApiController]
    [Route("utils")]
    [AutoValidateAntiforgeryToken]
    public class UtilsController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;

        public UtilsController(CachedUserManager<UserInfo> userManager)
        {
            this.userManager = userManager;
        }

        [Route("queryUsers")]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task<UserQueryResultListModel> QueryUser(string patterns)
        {
            var user = await userManager.GetUserAsync(User);
            IQueryable<UserInfo> users;
            if (Utils.PrivilegeHelper.IsTeacher(user.Privilege))
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
            }).Cacheable();

            return new UserQueryResultListModel
            {
                Users = await result.ToListAsync(),
                TotalCount = await result.CountAsync()
            };
        }
    }
}
