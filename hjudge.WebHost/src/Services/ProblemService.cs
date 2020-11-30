﻿using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Services
{
    public interface IProblemService
    {
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId);
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId);
        Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, int groupId);
        Task<Problem?> GetProblemAsync(int problemId);
        Task<int> CreateProblemAsync(Problem problem);
        Task UpdateProblemAsync(Problem problem);
        Task RemoveProblemAsync(int problemId);
    }
    public class ProblemService : IProblemService
    {
        private readonly UserManager<UserInfo> userManager;
        private readonly WebHostDbContext dbContext;
        private readonly IContestService contestService;
        private readonly IGroupService groupService;

        public ProblemService(
            UserManager<UserInfo> userManager,
            WebHostDbContext dbContext,
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

        public Task<Problem?> GetProblemAsync(int problemId)
        {
            return dbContext.Problem
                    .Where(i => i.Id == problemId)
                    .FirstOrDefaultAsync();
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Problem> problems = dbContext.Problem;

            if (!PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                problems = problems.Where(i => !i.Hidden);
            }

            return problems;
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var contest = await contestService.GetContestAsync(contestId);
            if (contest is null) throw new NotFoundException("找不到该比赛");

            if (!PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (contest.Hidden) throw new ForbiddenException();
            }

            IQueryable<Problem> problems = dbContext.ContestProblemConfig
                                                .Where(i => i.ContestId == contestId)
                                                .OrderBy(i => i.Id)
                                                .Select(i => i.Problem);

            return problems;
        }

        public async Task<IQueryable<Problem>> QueryProblemAsync(string? userId, int contestId, int groupId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var contest = await contestService.GetContestAsync(contestId);
            if (contest is null) throw new NotFoundException("找不到该比赛");

            var group = await groupService.GetGroupAsync(groupId);
            if (group is null) throw new NotFoundException("找不到该小组");

            if (!dbContext.GroupContestConfig.Any(i => i.GroupId == groupId && i.ContestId == contestId))
                throw new NotFoundException("找不到该比赛");

            if (!PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (contest.Hidden) throw new ForbiddenException();

                // user was not in this private group
                if (group.IsPrivate && !dbContext.GroupJoin.Any(i => i.GroupId == groupId && i.UserId == userId))
                    throw new ForbiddenException("未参加该小组");
            }

            IQueryable<Problem> problems = dbContext.ContestProblemConfig
                                                .Where(i => i.ContestId == contestId)
                                                .OrderBy(i => i.Id)
                                                .Select(i => i.Problem);

            return problems;
        }

        public async Task RemoveProblemAsync(int problemId)
        {
            var problem = await GetProblemAsync(problemId);
            if (problem is null) return;
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
