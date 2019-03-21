using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IProblemService
    {
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId, ApplicationDbContext dbContext);
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, ApplicationDbContext dbContext);
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, int groupId, ApplicationDbContext dbContext);
        Task<Problem> GetProblemAsync(int problemId, ApplicationDbContext dbContext);
        Task<int> CreateProblemAsync(Problem problem, ApplicationDbContext dbContext);
        Task UpdateProblemAsync(Problem problem, ApplicationDbContext dbContext);
        Task RemoveProblemAsync(int problemId, ApplicationDbContext dbContext);
    }
    public class ProblemService : IProblemService
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly ICacheService cacheService;

        public ProblemService(CachedUserManager<UserInfo> userManager, ICacheService cacheService)
        {
            this.userManager = userManager;
            this.cacheService = cacheService;
        }

        public async Task<int> CreateProblemAsync(Problem problem, ApplicationDbContext dbContext)
        {
            await dbContext.Problem.AddAsync(problem);
            await dbContext.SaveChangesAsync();
            return problem.Id;
        }

        public Task<Problem> GetProblemAsync(int problemId, ApplicationDbContext dbContext)
        {
            return cacheService.GetObjectAndSetAsync($"problem_{problemId}", () => dbContext.Problem.FirstOrDefaultAsync(i => i.Id == problemId));
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, ApplicationDbContext dbContext)
        {
            var user = await cacheService.GetObjectAndSetAsync($"user_{userId}", () => userManager.FindByIdAsync(userId));

            IQueryable<Problem> problems = dbContext.Problem;

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                problems = problems.Where(i => !i.Hidden);
            }

            return problems;
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, ApplicationDbContext dbContext)
        {
            var user = await cacheService.GetObjectAndSetAsync($"user_{userId}", () => userManager.FindByIdAsync(userId));

            var contest = await cacheService.GetObjectAndSetAsync($"contest_{contestId}", () => dbContext.Contest.FirstOrDefaultAsync(i => i.Id == contestId));
            if (contest == null) throw new InvalidOperationException("找不到比赛") { HResult = (int)ErrorDescription.ResourceNotFound };

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (contest.Hidden) throw new InvalidOperationException("") { HResult = (int)ErrorDescription.NoEnoughPrivilege };
            }

            IQueryable<Problem> problems = dbContext.ContestProblemConfig
                                                .Include(i => i.Problem)
                                                .Where(i => i.ContestId == contestId)
                                                .Select(i => i.Problem);

            return problems;
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, int groupId, ApplicationDbContext dbContext)
        {
            var user = await cacheService.GetObjectAndSetAsync($"user_{userId}", () => userManager.FindByIdAsync(userId));

            var contest = await cacheService.GetObjectAndSetAsync($"contest_{contestId}", () => dbContext.Contest.FirstOrDefaultAsync(i => i.Id == contestId));
            if (contest == null) throw new InvalidOperationException("找不到比赛") { HResult = (int)ErrorDescription.ResourceNotFound };

            var group = await cacheService.GetObjectAndSetAsync($"group_{groupId}", () => dbContext.Group.FirstOrDefaultAsync(i => i.Id == groupId));
            if (group == null) throw new InvalidOperationException("找不到小组") { HResult = (int)ErrorDescription.ResourceNotFound };

            if (!dbContext.GroupContestConfig.Any(i => i.GroupId == groupId && i.ContestId == contestId))
                throw new InvalidOperationException("找不到比赛") { HResult = (int)ErrorDescription.ResourceNotFound };

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (contest.Hidden) throw new InvalidOperationException("") { HResult = (int)ErrorDescription.NoEnoughPrivilege };

                // user was not in this private group
                if (group.IsPrivate && !dbContext.GroupJoin.Any(i => i.GroupId == groupId && i.UserId == userId))
                    throw new InvalidOperationException("未参加此小组") { HResult = (int)ErrorDescription.AuthenticationFailed };
            }

            IQueryable<Problem> problems = dbContext.ContestProblemConfig
                                                .Include(i => i.Problem)
                                                .Where(i => i.ContestId == contestId)
                                                .Select(i => i.Problem);

            return problems;
        }

        public async Task RemoveProblemAsync(int problemId, ApplicationDbContext dbContext)
        {
            var problem = await GetProblemAsync(problemId, dbContext);
            dbContext.Problem.Remove(problem);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateProblemAsync(Problem problem, ApplicationDbContext dbContext)
        {
            dbContext.Problem.Update(problem);
            await dbContext.SaveChangesAsync();
        }
    }
}
