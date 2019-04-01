using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IContestService
    {
        Task<IQueryable<Contest>> QueryContestAsync(string? userId);
        Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId);
        Task<Contest> GetContestAsync(int contestId);
        Task<int> CreateContestAsync(Contest contest);
        Task UpdateContestAsync(Contest contest);
        Task RemoveContestAsync(int contestId);
        Task UpdateContestProblemsAsync(int contestId, IEnumerable<int> problems);
    }
    public class ContestService : IContestService
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly ICacheService cacheService;
        private readonly ApplicationDbContext dbContext;

        public ContestService(CachedUserManager<UserInfo> userManager, ICacheService cacheService, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.cacheService = cacheService;
            this.dbContext = dbContext;
        }

        public async Task<int> CreateContestAsync(Contest contest)
        {
            await dbContext.Contest.AddAsync(contest);
            await dbContext.SaveChangesAsync();
            await cacheService.SetObjectAsync($"contest_{contest.Id}", contest);
            return contest.Id;
        }

        public Task<Contest> GetContestAsync(int contestId)
        {
            return cacheService.GetObjectAndSetAsync($"contest_{contestId}", () => dbContext.Contest.FirstOrDefaultAsync(i => i.Id == contestId));
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId)
        {
            var user = await cacheService.GetObjectAndSetAsync($"user_{userId}", () => userManager.FindByIdAsync(userId));

            IQueryable<Contest> contests = dbContext.Contest;

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                contests = contests.Where(i => !i.Hidden);
            }
            return contests;
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId)
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

        public async Task RemoveContestAsync(int contestId)
        {
            var contest = await GetContestAsync(contestId);
            dbContext.Contest.Remove(contest);
            await dbContext.SaveChangesAsync();
            await cacheService.RemoveObjectAsync($"contest_{contestId}");
        }

        public async Task UpdateContestAsync(Contest contest)
        {
            dbContext.Contest.Update(contest);
            await dbContext.SaveChangesAsync();
            await cacheService.RemoveObjectAsync($"contest_{contest.Id}");
            await cacheService.SetObjectAsync($"contest_{contest.Id}", contest);
        }

        public async Task UpdateContestProblemsAsync(int contestId, IEnumerable<int> problems)
        {
            var oldProblems = await dbContext.ContestProblemConfig.Where(i => i.ContestId == contestId).ToListAsync();
            dbContext.ContestProblemConfig.RemoveRange(oldProblems);
            var dict = oldProblems.ToDictionary(i => i.ProblemId);
            foreach(var i in problems)
            {
                if (dict.ContainsKey(i))
                {
                    dict[i].Id = 0;
                    dbContext.ContestProblemConfig.Add(dict[i]);
                }
                else
                {
                    dbContext.ContestProblemConfig.Add(new ContestProblemConfig
                    {
                        ProblemId = i,
                        ContestId = contestId,
                        AcceptCount = 0,
                        SubmissionCount = 0
                    });
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
