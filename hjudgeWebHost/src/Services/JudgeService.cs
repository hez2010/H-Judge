using hjudgeCore;
using hjudgeWebHost.Data;
using hjudgeWebHost.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IJudgeService
    {
        Task<IQueryable<Judge>> QueryJudgesAsync(int? groupId, int? contestId, int? problemId);
        Task<IQueryable<Judge>> QueryJudgesAsync(string userId, int? groupId, int? contestId, int? problemId);
        Task<Judge> GetJudgeAsync(int judgeId);
        Task QueueJudgeAsync(Judge judge);
        Task UpdateJudgeResultAsync(int judgeId, JudgeResult judge);
    }
    public class JudgeService : IJudgeService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ICacheService cacheService;
        private readonly IProblemService problemService;
        private readonly ILanguageService languageService;
        private readonly IMessageQueueService messageQueueService;

        public JudgeService(ApplicationDbContext dbContext,
            ICacheService cacheService,
            IProblemService problemService,
            ILanguageService languageService,
            IMessageQueueService messageQueueService)
        {
            this.dbContext = dbContext;
            this.cacheService = cacheService;
            this.problemService = problemService;
            this.languageService = languageService;
            this.messageQueueService = messageQueueService;
        }

        public Task<Judge> GetJudgeAsync(int judgeId)
        {
            return cacheService.GetObjectAndSetAsync($"judge_{judgeId}", () => dbContext.Judge.FirstOrDefaultAsync(i => i.Id == judgeId));
        }

        public Task<IQueryable<Judge>> QueryJudgesAsync(int? groupId, int? contestId, int? problemId)
        {
            return Task.FromResult(problemId switch
            {
                null => dbContext.Judge.Where(i => i.GroupId == null && i.ContestId == null),
                _ => dbContext.Judge.Where(i => i.GroupId == null && i.ContestId == null && i.ProblemId == problemId)
            });
        }

        public Task<IQueryable<Judge>> QueryJudgesAsync(string userId, int? groupId, int? contestId, int? problemId)
        {
            return Task.FromResult(problemId switch
            {
                null => dbContext.Judge.Where(i => i.UserId == userId && i.GroupId == groupId && i.ContestId == contestId),
                _ => dbContext.Judge.Where(i => i.UserId == userId && i.GroupId == groupId && i.ContestId == contestId && i.ProblemId == problemId)
            });
        }

        public async Task QueueJudgeAsync(Judge judge)
        {
            var (judgeOptionBuilder, buildOptionBuilder) = await JudgeHelper.GetOptionBuilders(problemService, judge, await languageService.GetLanguageConfigAsync());
            var (judgeConfig, buildConfig) = (judgeOptionBuilder.Build(), buildOptionBuilder.Build());

            var (channel, options) = messageQueueService.GetInstance("JudgeQueue");
            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;
            channel.BasicPublish(
                options.Exchange,
                options.RoutingKey,
                false,
                props,
                new { JudgeConfig = judgeConfig, BuildConfig = buildConfig }.SerializeJson());
        }

        public async Task UpdateJudgeResultAsync(int judgeId, JudgeResult result)
        {
            var judge = await GetJudgeAsync(judgeId);
            judge.Result = result.SerializeJsonAsString();
            judge.ResultType = (int)new Func<ResultCode>(() =>
            {
                if (result.JudgePoints == null)
                {
                    return ResultCode.Judging;
                }

                if (result.JudgePoints.Count == 0 || result.JudgePoints.All(i => i.ResultType == ResultCode.Accepted))
                {
                    return ResultCode.Accepted;
                }

                var mostPresentTimes =
                    result.JudgePoints.Select(i => i.ResultType).Distinct().Max(i =>
                        result.JudgePoints.Count(j => j.ResultType == i && j.ResultType != ResultCode.Accepted));
                var mostPresent =
                    result.JudgePoints.Select(i => i.ResultType).Distinct().FirstOrDefault(
                        i => result.JudgePoints.Count(j => j.ResultType == i && j.ResultType != ResultCode.Accepted) ==
                             mostPresentTimes
                    );
                return mostPresent;
            }).Invoke();
            judge.FullScore = result.JudgePoints?.Sum(i => i.Score) ?? 0;
        }
    }
}
