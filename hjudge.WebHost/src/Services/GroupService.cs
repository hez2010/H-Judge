using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Services
{
    public interface IGroupService
    {
        Task<IQueryable<Group>> QueryGroupAsync(string? userId);
        Task<Group?> GetGroupAsync(int groupId);
        Task<int> CreateGroupAsync(Group group);
        Task UpdateGroupAsync(Group group);
        Task RemoveGroupAsync(int groupId);
        Task UpdateGroupContestAsync(int groupId, IEnumerable<int> contests);
        Task<bool> OptInGroupAsync(string userId, int groupId);
        Task<bool> OptOutGroupAsync(string userId, int groupId);
        Task<IQueryable<GroupJoin>> QueryGroupJoinRecordsAsync();
        Task<bool> IsInGroupAsync(string userId, int groupId);
    }
    public class GroupService : IGroupService
    {
        private readonly WebHostDbContext dbContext;
        private readonly UserManager<UserInfo> userManager;

        public GroupService(WebHostDbContext dbContext, UserManager<UserInfo> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        public async Task<int> CreateGroupAsync(Group group)
        {
            await dbContext.Group.AddAsync(group);
            await dbContext.SaveChangesAsync();
            return group.Id;
        }

        public Task<Group?> GetGroupAsync(int groupId)
        {
            return dbContext.Group
                    .Where(i => i.Id == groupId)
                    .FirstOrDefaultAsync();
        }

        public Task<bool> IsInGroupAsync(string userId, int groupId)
        {
            return dbContext.GroupJoin.Where(i => i.UserId == userId && i.GroupId == groupId).AnyAsync();
        }

        public async Task<bool> OptInGroupAsync(string userId, int groupId)
        {
            var user = await userManager.FindByIdAsync(userId);
            var group = await GetGroupAsync(groupId);
            if (user != null && group != null)
            {
                if (!await dbContext.GroupJoin.Where(i => i.GroupId == groupId && i.UserId == userId).AnyAsync())
                {
                    await dbContext.GroupJoin.AddAsync(new GroupJoin
                    {
                        GroupId = groupId,
                        UserId = userId,
                        JoinTime = DateTime.Now
                    });
                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> OptOutGroupAsync(string userId, int groupId)
        {
            var user = await userManager.FindByIdAsync(userId);
            var group = await GetGroupAsync(groupId);
            if (user != null && group != null)
            {
                var info = await dbContext.GroupJoin.Where(i => i.GroupId == groupId && i.UserId == userId).FirstOrDefaultAsync();
                if (info != null)
                {
                    dbContext.GroupJoin.Remove(info);
                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<IQueryable<Group>> QueryGroupAsync(string? userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Group> groups = dbContext.Group;

            if (!PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                groups = groups.Where(i => (i.IsPrivate && i.GroupJoin.Any(j => j.GroupId == i.Id && j.UserId == userId)) || !i.IsPrivate);
            }

            return groups;
        }

        public Task<IQueryable<GroupJoin>> QueryGroupJoinRecordsAsync()
        {
            IQueryable<GroupJoin> groups = dbContext.GroupJoin;
            return Task.FromResult(groups);
        }

        public async Task RemoveGroupAsync(int groupId)
        {
            var group = await GetGroupAsync(groupId);
            if (group is null) return;
            dbContext.Group.Remove(group);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateGroupAsync(Group group)
        {
            dbContext.Group.Update(group);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateGroupContestAsync(int groupId, IEnumerable<int> contests)
        {
            var oldContests = await dbContext.GroupContestConfig.Where(i => i.GroupId == groupId).ToListAsync();
            dbContext.GroupContestConfig.RemoveRange(oldContests);
            var dict = oldContests.ToDictionary(i => i.ContestId);
            foreach (var i in contests.Distinct())
            {
                if (dict.ContainsKey(i))
                {
                    dict[i].Id = 0;
                    await dbContext.GroupContestConfig.AddAsync(dict[i]);
                }
                else
                {
                    await dbContext.GroupContestConfig.AddAsync(new GroupContestConfig
                    {
                        ContestId = i,
                        GroupId = groupId
                    });
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
