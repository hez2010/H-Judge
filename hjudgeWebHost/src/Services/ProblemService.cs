using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.AspNetCore.Identity;
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
    }
    public class ProblemService : IProblemService
    {
        private readonly UserManager<UserInfo> userManager;

        public ProblemService(UserManager<UserInfo> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, ApplicationDbContext dbContext)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Problem> problems = dbContext.Problem;

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                problems = problems.Where(i => !i.Hidden);
            }

            return problems;
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, ApplicationDbContext dbContext)
        {
            var user = await userManager.FindByIdAsync(userId);

            var contest = await dbContext.Contest.FirstOrDefaultAsync(i => i.Id == contestId);
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
            var user = await userManager.FindByIdAsync(userId);

            var contest = await dbContext.Contest.FirstOrDefaultAsync(i => i.Id == contestId);
            if (contest == null) throw new InvalidOperationException("找不到比赛") { HResult = (int)ErrorDescription.ResourceNotFound };

            var group = await dbContext.Group.FirstOrDefaultAsync(i => i.Id == groupId);
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
    }
}
