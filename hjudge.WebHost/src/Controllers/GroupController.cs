using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Models.Group;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using hjudge.WebHost.Exceptions;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("group")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService groupService;
        private readonly CachedUserManager<UserInfo> userManager;

        public GroupController(
            IGroupService groupService,
            CachedUserManager<UserInfo> userManager
            )
        {
            this.groupService = groupService;
            this.userManager = userManager;
        }

        private readonly static int[] allStatus = new[] { 0, 1 };

        [Route("list")]
        public async Task<GroupListModel> GroupList([FromBody]GroupListQueryModel model)
        {
            var userId = userManager.GetUserId(User);
            var groups = await groupService.QueryGroupAsync(userId);

            var ret = new GroupListModel();

            if (model.Filter.Id != 0)
            {
                groups = groups.Where(i => i.Id == model.Filter.Id);
            }

            if (!string.IsNullOrEmpty(model.Filter.Name))
            {
                groups = groups.Where(i => i.Name.Contains(model.Filter.Name));
            }

            var groupJoin = await groupService.QueryGroupJoinRecordsAsync();

            if (model.Filter.Status.Count < 2)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    foreach (var status in allStatus)
                    {
                        if (!model.Filter.Status.Contains(status))
                        {
                            groups = status switch
                            {
                                0 => groups.Where(i => groupJoin.Any(j => j.GroupId == i.Id && j.UserId == userId)),
                                1 => groups.Where(i => !groupJoin.Any(j => j.GroupId == i.Id && j.UserId == userId)),
                                _ => groups
                            };
                        }
                    }
                }
            }

            if (model.RequireTotalCount) ret.TotalCount = await groups.Select(i => i.Id)/*.Cacheable()*/.CountAsync();

            groups = groups.OrderByDescending(i => i.Id);
            if (model.StartId == 0) groups = groups.Skip(model.Start);
            else groups = groups.Where(i => i.Id <= model.StartId);

            ret.Groups = await groups.Take(model.Count).Include(i => i.UserInfo).Select(i => new GroupListModel.GroupListItemModel
            {
                Id = i.Id,
                CreationTime = i.CreationTime,
                IsPrivate = i.IsPrivate,
                Name = i.Name,
                UserId = i.UserId,
                UserName = i.UserInfo.UserName
            })/*.Cacheable()*/.ToListAsync();

            return ret;
        }

        [Route("details")]
        [HttpGet]
        public async Task<GroupModel> GroupDetails(int groupId)
        {
            var user = await userManager.GetUserAsync(User);
            var groups = await groupService.QueryGroupAsync(user?.Id);
            var group = await groups.Where(i => i.Id == groupId).FirstOrDefaultAsync();
            if (group == null) throw new NotFoundException("该小组不存在");

            return new GroupModel
            {
                Id = group.Id,
                UserId = group.UserId,
                UserName = group.UserInfo.UserName,
                Name = group.Name,
                Description = group.Description,
                IsPrivate = group.IsPrivate
            };
        }
    }
}
