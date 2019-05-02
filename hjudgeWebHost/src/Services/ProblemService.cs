using EFSecondLevelCache.Core;
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
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId);
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId);
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, int groupId);
        Task<Problem> GetProblemAsync(int problemId);
        Task<int> CreateProblemAsync(Problem problem);
        Task UpdateProblemAsync(Problem problem);
        Task RemoveProblemAsync(int problemId);
    }
    public class ProblemService : IProblemService
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly ApplicationDbContext dbContext;
        private readonly IContestService contestService;
        private readonly IGroupService groupService;

        public ProblemService(
            CachedUserManager<UserInfo> userManager,
            ApplicationDbContext dbContext,
            IContestService contestService,
            IGroupService groupService)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.contestService = contestService;
            this.groupService = groupService;
        }

        public async Task<int> CreateProblemAsync(Problem problem)
        {
            await dbContext.Problem.AddAsync(problem);
            await dbContext.SaveChangesAsync();
            return problem.Id;
        }

        public Task<Problem> GetProblemAsync(int problemId)
        {
            return dbContext.Problem.Cacheable().FirstOrDefaultAsync(i => i.Id == problemId);
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Problem> problems = dbContext.Problem;

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                problems = problems.Where(i => !i.Hidden);
            }

            return problems;
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var contest = await contestService.GetContestAsync(contestId);
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

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, int groupId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var contest = await contestService.GetContestAsync(contestId);
            if (contest == null) throw new InvalidOperationException("找不到比赛") { HResult = (int)ErrorDescription.ResourceNotFound };

            var group = await groupService.GetGroupAsync(groupId);
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

        public async Task RemoveProblemAsync(int problemId)
        {
            var problem = await GetProblemAsync(problemId);
            dbContext.Problem.Remove(problem);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateProblemAsync(Problem problem)
        {
            dbContext.Problem.Update(problem);
            await dbContext.SaveChangesAsync();
        }
    }
}
