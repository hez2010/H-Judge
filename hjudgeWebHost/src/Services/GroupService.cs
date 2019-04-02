using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IGroupService
    {
        Task<IQueryable<Group>> QueryGroupAsync(string? userId);
        Task<Group> GetGroupAsync(int groupId);
        Task<int> CreateGroupAsync(Group group);
        Task UpdateGroupAsync(Group group);
        Task RemoveGroupAsync(int groupId);
        Task UpdateGroupContestAsync(int groupId, IEnumerable<int> contests);
        Task<IQueryable<GroupContestConfig>> QueryGroupContestAsync(int groupId);
    }
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ICacheService cacheService;
        private readonly CachedUserManager<UserInfo> userManager;

        public GroupService(ApplicationDbContext dbContext, ICacheService cacheService, CachedUserManager<UserInfo> userManager)
        {
            this.dbContext = dbContext;
            this.cacheService = cacheService;
            this.userManager = userManager;
        }
        public async Task<int> CreateGroupAsync(Group group)
        {
            await dbContext.Group.AddAsync(group);
            await dbContext.SaveChangesAsync();
            await cacheService.SetObjectAsync($"group_{group.Id}", group);
            return group.Id;
        }

        public Task<Group> GetGroupAsync(int groupId)
        {
            return cacheService.GetObjectAndSetAsync($"group_{groupId}", () => dbContext.Group.FirstOrDefaultAsync(i => i.Id == groupId));
        }

        public async Task<IQueryable<Group>> QueryGroupAsync(string? userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Group> groups = dbContext.Group.Include(i => i.GroupJoin);

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                groups = groups.Where(i => (i.IsPrivate && i.GroupJoin.Any(j => j.GroupId == i.Id && j.UserId == userId)) || !i.IsPrivate);
            }

            return groups;
        }

        public Task<IQueryable<GroupContestConfig>> QueryGroupContestAsync(int groupId)
        {
            return Task.FromResult(dbContext.GroupContestConfig
                .Include(i => i.Group)
                .Where(i => i.GroupId == groupId));
        }

        public async Task RemoveGroupAsync(int groupId)
        {
            var group = await GetGroupAsync(groupId);
            dbContext.Group.Remove(group);
            await dbContext.SaveChangesAsync();
            await cacheService.RemoveObjectAsync($"group_{group.Id}");
        }

        public async Task UpdateGroupAsync(Group group)
        {
            dbContext.Group.Update(group);
            await dbContext.SaveChangesAsync();
            await cacheService.RemoveObjectAsync($"group_{group.Id}");
            await cacheService.SetObjectAsync($"group_{group.Id}", group);
        }

        public async Task UpdateGroupContestAsync(int groupId, IEnumerable<int> contests)
        {
            var oldContests = await dbContext.GroupContestConfig.Where(i => i.GroupId == groupId).ToListAsync();
            dbContext.GroupContestConfig.RemoveRange(oldContests);
            var dict = oldContests.ToDictionary(i => i.ContestId);
            foreach (var i in contests)
            {
                if (dict.ContainsKey(i))
                {
                    dict[i].Id = 0;
                    dbContext.GroupContestConfig.Add(dict[i]);
                }
                else
                {
                    dbContext.GroupContestConfig.Add(new GroupContestConfig
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
