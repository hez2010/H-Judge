using EFSecondLevelCache.Core;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudge.WebHost.Services
{
    public interface IContestService
    {
        Task<IQueryable<Contest>> QueryContestAsync(string? userId);
        Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId);
        Task<Contest> GetContestAsync(int contestId);
        Task<int> CreateContestAsync(Contest contest);
        Task UpdateContestAsync(Contest contest);
        Task RemoveContestAsync(int contestId);
        Task UpdateContestProblemAsync(int contestId, IEnumerable<int> problems);
    }
    public class ContestService : IContestService
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IGroupService groupService;
        private readonly WebHostDbContext dbContext;

        public ContestService(CachedUserManager<UserInfo> userManager,
            IGroupService groupService,
            WebHostDbContext dbContext)
        {
            this.userManager = userManager;
            this.groupService = groupService;
            this.dbContext = dbContext;
        }

        public async Task<int> CreateContestAsync(Contest contest)
        {
            await dbContext.Contest.AddAsync(contest);
            await dbContext.SaveChangesAsync();
            return contest.Id;
        }

        public async Task<Contest> GetContestAsync(int contestId)
        {
            var result = await dbContext.Contest.Cacheable().FirstOrDefaultAsync(i => i.Id == contestId);
            if (result != null)
            {
                dbContext.Entry(result).State = EntityState.Detached;
            }
            return result;
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Contest> contests = dbContext.Contest.Include(i => i.ContestRegister);

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                contests = contests.Where(i => !i.Hidden || (i.SpecifyCompetitors && i.ContestRegister.Any(j => j.ContestId == i.Id && j.UserId == userId)));
            }
            return contests;
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var group = await groupService.GetGroupAsync(groupId);
            if (group == null) throw new InvalidOperationException("找不到小组") { HResult = (int)ErrorDescription.ResourceNotFound };

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (group.IsPrivate)
                {
                    if (!dbContext.GroupJoin.Any(i => i.GroupId == groupId && i.UserId == userId))
                        throw new InvalidOperationException("未参加此小组") { HResult = (int)ErrorDescription.NoEnoughPrivilege };
                }
            }

            IQueryable<Contest> contests = dbContext.GroupContestConfig
                .Include(i => i.Contest).Where(i => i.GroupId == groupId).Select(i => i.Contest);

            return contests;
        }

        public async Task RemoveContestAsync(int contestId)
        {
            var contest = await GetContestAsync(contestId);
            dbContext.Contest.Remove(contest);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateContestAsync(Contest contest)
        {
            dbContext.Contest.Update(contest);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateContestProblemAsync(int contestId, IEnumerable<int> problems)
        {
            var oldProblems = await dbContext.ContestProblemConfig.Where(i => i.ContestId == contestId).Cacheable().ToListAsync();
            dbContext.ContestProblemConfig.RemoveRange(oldProblems);
            var dict = oldProblems.ToDictionary(i => i.ProblemId);
            foreach (var i in problems.Distinct())
            {
                if (dict.ContainsKey(i))
                {
                    dict[i].Id = 0;
                    await dbContext.ContestProblemConfig.AddAsync(dict[i]);
                }
                else
                {
                    await dbContext.ContestProblemConfig.AddAsync(new ContestProblemConfig
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
