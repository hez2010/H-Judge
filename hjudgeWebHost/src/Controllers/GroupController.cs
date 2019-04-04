using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Models.Group;
using hjudgeWebHost.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
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

        public class GroupListQueryModel
        {
            public class GroupFilter
            {
                public int Id { get; set; } = 0;
                public string Name { get; set; } = string.Empty;
            }
            public int Start { get; set; }
            public int StartId { get; set; }
            public int Count { get; set; }
            public bool RequireTotalCount { get; set; }
            public GroupFilter Filter { get; set; } = new GroupFilter();
        }

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
            }).ToListAsync();

            if (model.RequireTotalCount)
            {
                ret.TotalCount = await groups.CountAsync();
            }
            return ret;
        }
    }
}
