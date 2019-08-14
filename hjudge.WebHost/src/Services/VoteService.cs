using hjudge.WebHost.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Services
{
    public interface IVoteService
    {
        Task<bool> UpvoteProblemAsync(string userId, int problemId, string? title = null, string? content = null);
        Task<bool> UpvoteContestAsync(string userId, int contestId, string? title = null, string? content = null);
        Task<bool> DownvoteProblemAsync(string userId, int problemId, string? title = null, string? content = null);
        Task<bool> DownvoteContestAsync(string userId, int contestId, string? title = null, string? content = null);
    }
    public class VoteService : IVoteService
    {
        private readonly IProblemService problemService;
        private readonly IContestService contestService;
        private readonly WebHostDbContext dbContext;

        public VoteService(IProblemService problemService, IContestService contestService, WebHostDbContext dbContext)
        {
            this.problemService = problemService;
            this.contestService = contestService;
            this.dbContext = dbContext;
        }

        public async Task<bool> DownvoteContestAsync(string userId, int contestId, string? title = null, string? content = null)
        {
            var contest = await contestService.GetContestAsync(contestId);
            if (contest == null) return false;
            var exists = await dbContext.VotesRecord.Where(i => i.UserId == userId && i.ProblemId == null && i.ContestId == contestId).AnyAsync();
            if (exists) return false;
            ++contest.Downvote;
            await dbContext.VotesRecord.AddAsync(new VotesRecord
            {
                ContestId = contestId,
                UserId = userId,
                ProblemId = null,
                VoteType = 1,
                VoteTime = DateTime.Now,
                Title = title ?? string.Empty,
                Content = string.IsNullOrEmpty(title) ? string.Empty : (content ?? string.Empty)
            });
            dbContext.Contest.Update(contest);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DownvoteProblemAsync(string userId, int problemId, string? title = null, string? content = null)
        {
            var problem = await problemService.GetProblemAsync(problemId);
            if (problem == null) return false;
            var exists = await dbContext.VotesRecord.Where(i => i.UserId == userId && i.ProblemId == problemId && i.ContestId == null).AnyAsync();
            if (exists) return false;
            ++problem.Downvote;
            await dbContext.VotesRecord.AddAsync(new VotesRecord
            {
                ProblemId = problemId,
                UserId = userId,
                ContestId = null,
                VoteType = 1,
                VoteTime = DateTime.Now,
                Title = title ?? string.Empty,
                Content = string.IsNullOrEmpty(title) ? string.Empty : (content ?? string.Empty)
            });
            dbContext.Problem.Update(problem);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpvoteContestAsync(string userId, int contestId, string? title = null, string? content = null)
        {
            var contest = await contestService.GetContestAsync(contestId);
            if (contest == null) return false;
            var exists = await dbContext.VotesRecord.Where(i => i.UserId == userId && i.ProblemId == null && i.ContestId == contestId).AnyAsync();
            if (exists) return false;
            ++contest.Upvote;
            await dbContext.VotesRecord.AddAsync(new VotesRecord
            {
                ContestId = contestId,
                UserId = userId,
                ProblemId = null,
                VoteType = 0,
                VoteTime = DateTime.Now,
                Title = title ?? string.Empty,
                Content = string.IsNullOrEmpty(title) ? string.Empty : (content ?? string.Empty)
            });
            dbContext.Contest.Update(contest);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpvoteProblemAsync(string userId, int problemId, string? title = null, string? content = null)
        {
            var problem = await problemService.GetProblemAsync(problemId);
            if (problem == null) return false;
            var exists = await dbContext.VotesRecord.Where(i => i.UserId == userId && i.ProblemId == problemId && i.ContestId == null).AnyAsync();
            if (exists) return false;
            ++problem.Upvote;
            await dbContext.VotesRecord.AddAsync(new VotesRecord
            {
                ProblemId = problemId,
                UserId = userId,
                ContestId = null,
                VoteType = 0,
                VoteTime = DateTime.Now,
                Title = title ?? string.Empty,
                Content = string.IsNullOrEmpty(title) ? string.Empty : (content ?? string.Empty)
            });
            dbContext.Problem.Update(problem);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}
