using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Services
{
    public interface IContestService
    {
        Task<IQueryable<Contest>> QueryContestAsync(string? userId);
        Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId);
        Task<Contest?> GetContestAsync(int contestId);
        Task<int> CreateContestAsync(Contest contest);
        Task UpdateContestAsync(Contest contest);
        Task RemoveContestAsync(int contestId);
        Task UpdateContestProblemAsync(int contestId, IEnumerable<int> problems);
        Task JoinContestAsync(int contestId, string[] userId);
        Task QuitContestAsync(int contestId, string[] userId);
        Task<bool> HasJoinedContestAsync(int contestId, string userId);
        Task<IQueryable<Contest>> QueryJoinedContestAsync(string userId);
        Task<IQueryable<UserInfo>> QueryCompetitorsAsync(int contestId);
    }
    public class ContestService : IContestService
    {
        private readonly UserManager<UserInfo> userManager;
        private readonly IGroupService groupService;
        private readonly WebHostDbContext dbContext;

        public ContestService(UserManager<UserInfo> userManager,
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

        public Task<Contest?> GetContestAsync(int contestId)
        {
            return dbContext.Contest
                .Where(i => i.Id == contestId)
                .FirstOrDefaultAsync();
        }

        public Task<bool> HasJoinedContestAsync(int contestId, string userId)
        {
            return dbContext.ContestRegister
                .AnyAsync(i => i.ContestId == contestId && i.UserId == userId);
        }

        public async Task JoinContestAsync(int contestId, string[] userId)
        {
            foreach (var i in userId)
            {
                if (await HasJoinedContestAsync(contestId, i)) continue;
                await dbContext.ContestRegister
                    .AddAsync(new ContestRegister
                    {
                        UserId = i,
                        ContestId = contestId
                    });
            }
            await dbContext.SaveChangesAsync();
        }

        public Task<IQueryable<UserInfo>> QueryCompetitorsAsync(int contestId)
        {
            return Task.FromResult(dbContext.ContestRegister.Where(i => i.ContestId == contestId)
                .Select(i => i.UserInfo));
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Contest> contests = dbContext.Contest;

            if (!PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                contests = contests.Where(i => !i.Hidden || (i.SpecifyCompetitors && i.ContestRegister.Any(j => j.ContestId == i.Id && j.UserId == userId)));
            }
            return contests;
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var group = await groupService.GetGroupAsync(groupId);
            if (group is null) throw new NotFoundException("找不到该小组");

            if (!PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (group.IsPrivate)
                {
                    if (!dbContext.GroupJoin.Any(i => i.GroupId == groupId && i.UserId == userId)) throw new ForbiddenException("未参加该小组");
                }
            }

            IQueryable<Contest> contests = dbContext.GroupContestConfig
                .Where(i => i.GroupId == groupId).OrderByDescending(i => i.Id).Select(i => i.Contest);

            return contests;
        }

        public Task<IQueryable<Contest>> QueryJoinedContest(string userId)
        {
            return Task.FromResult(dbContext.ContestRegister.
                Where(i => i.UserId == userId).Select(i => i.Contest));
        }

        public Task<IQueryable<Contest>> QueryJoinedContestAsync(string userId)
        {
            throw new System.NotImplementedException();
        }

        public async Task QuitContestAsync(int contestId, string[] userId)
        {
            var registerInfo = dbContext.ContestRegister
                .Where(i => i.ContestId == contestId && userId.Contains(i.UserId));
            if (registerInfo is null) return;
            dbContext.ContestRegister.RemoveRange(registerInfo);
            await dbContext.SaveChangesAsync();
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
            var oldProblems = await dbContext.ContestProblemConfig.Where(i => i.ContestId == contestId).ToListAsync();

            var dict = new Dictionary<int, ContestProblemConfig>();

            foreach (var i in oldProblems) if (!dict.ContainsKey(i.ProblemId)) dict[i.ProblemId] = i;

            dbContext.ContestProblemConfig.RemoveRange(oldProblems);
            await dbContext.SaveChangesAsync();

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
