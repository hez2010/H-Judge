using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IContestService
    {
        Task<IQueryable<Contest>> QueryContestAsync(string? userId, ApplicationDbContext dbContext);
        Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId, ApplicationDbContext dbContext);
        Task<Contest> GetContestAsync(int contestId, ApplicationDbContext dbContext);
        Task CreateContestAsync(Contest contest);
        Task<bool> UpdateContestAsync(Contest contest);
        Task RemoveContestAsync(int contestId);
    }
    public class ContestService : IContestService
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly ICacheService cacheService;

        public ContestService(CachedUserManager<UserInfo> userManager, ICacheService cacheService)
        {
            this.userManager = userManager;
            this.cacheService = cacheService;
        }

        public Task CreateContestAsync(Contest contest)
        {
            throw new NotImplementedException();
        }

        public Task<Contest> GetContestAsync(int contestId, ApplicationDbContext dbContext)
        {
            return cacheService.GetObjectAndSetAsync($"contest_{contestId}", () => dbContext.Contest.FirstOrDefaultAsync(i => i.Id == contestId));
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId, ApplicationDbContext dbContext)
        {
            var user = await cacheService.GetObjectAndSetAsync($"user_{userId}", () => userManager.FindByIdAsync(userId));

            IQueryable<Contest> contests = dbContext.Contest;

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                contests = contests.Where(i => !i.Hidden);
            }
            return contests;
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId, ApplicationDbContext dbContext)
        {
            var user = await cacheService.GetObjectAndSetAsync($"user_{userId}", () => userManager.FindByIdAsync(userId));

            var group = await cacheService.GetObjectAndSetAsync($"group_{groupId}", () => dbContext.Group.FirstOrDefaultAsync(i => i.Id == groupId));
            if (group == null) throw new InvalidOperationException("找不到小组") { HResult = (int)ErrorDescription.ResourceNotFound };

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (group.IsPrivate)
                {
                    if (!dbContext.GroupJoin.Any(i => i.GroupId == groupId && i.UserId == userId))
                        throw new InvalidOperationException("未参加此小组") { HResult = (int)ErrorDescription.NoEnoughPrivilege };
                }
            }

            IQueryable<Contest> contests = dbContext.GroupContestConfig.Include(i => i.Contest).Where(i => i.GroupId == groupId).Select(i => i.Contest);

            return contests;
        }

        public Task RemoveContestAsync(int contestId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateContestAsync(Contest contest)
        {
            throw new NotImplementedException();
        }
    }
}
