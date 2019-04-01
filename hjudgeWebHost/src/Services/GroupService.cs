using hjudgeWebHost.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IGroupService
    {
        Task<IQueryable<Group>> QueryGroupAsync();
        Task<IQueryable<Group>> QueryGroupAsync(string userId);
        Task<Group> GetGroupAsync(int groupId);
        Task<int> CreateGroupAsync(Group group);
        Task UpdateGroupAsync(Group group);
        Task RemoveGroupAsync(int groupId);
        Task UpdateGroupContestsAsync(int groupId, IEnumerable<int> contests);
    }
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ICacheService cacheService;

        public GroupService(ApplicationDbContext dbContext, ICacheService cacheService)
        {
            this.dbContext = dbContext;
            this.cacheService = cacheService;
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

        public Task<IQueryable<Group>> QueryGroupAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<Group>> QueryGroupAsync(string userId)
        {
            throw new NotImplementedException();
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

        public async Task UpdateGroupContestsAsync(int groupId, IEnumerable<int> contests)
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
