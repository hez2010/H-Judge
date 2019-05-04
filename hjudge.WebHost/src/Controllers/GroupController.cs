﻿using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Models.Group;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
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
            }).Cacheable().ToListAsync();

            if (model.RequireTotalCount) ret.TotalCount = await groups.Select(i => i.Id).Cacheable().CountAsync();
            return ret;
        }
    }
}