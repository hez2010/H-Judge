using EFSecondLevelCache.Core;
using hjudge.Core;
using hjudge.Shared.Judge;
using hjudge.Shared.Utils;
using hjudge.WebHost.Data;
using hjudge.WebHost.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data.Identity;
using static hjudge.Shared.Judge.JudgeInfo;

namespace hjudge.WebHost.Services
{
    public interface IJudgeService
    {
        Task<IQueryable<Judge>> QueryJudgesAsync(string? userId = null, int? groupId = 0, int? contestId = 0, int? problemId = 0, int? resultType = null);
        Task<Judge?> GetJudgeAsync(int judgeId, bool includeForeignFields = false);
        Task<int> QueueJudgeAsync(Judge judge);
        Task UpdateJudgeResultAsync(int judgeId, JudgeReportInfo.ReportType reportType, JudgeResult? judge);
    }
    public class JudgeService : IJudgeService
    {
        private readonly WebHostDbContext dbContext;
        private readonly IProblemService problemService;
        private readonly ILanguageService languageService;
        private readonly IMessageQueueService messageQueueService;
        private readonly IContestService contestService;
        private readonly CachedUserManager<UserInfo> userManager;

        public JudgeService(WebHostDbContext dbContext,
            IProblemService problemService,
            ILanguageService languageService,
            IMessageQueueService messageQueueService,
            IContestService contestService,
            CachedUserManager<UserInfo> userManager)
        {
            this.dbContext = dbContext;
            this.problemService = problemService;
            this.languageService = languageService;
            this.messageQueueService = messageQueueService;
            this.contestService = contestService;
            this.userManager = userManager;
        }

        public async Task<Judge?> GetJudgeAsync(int judgeId, bool includeForeignFields = false)
        {
            var judges = includeForeignFields ? dbContext.Judge
                .Include(i => i.Problem)
                .Include(i => i.UserInfo)
                .Include(i => i.Contest)
                .Include(i => i.Group)
                .AsNoTracking() : dbContext.Judge.AsNoTracking();
                
            var result = await judges
                .Where(i => i.Id == judgeId)
                /*.Cacheable()*/
                .FirstOrDefaultAsync();
            return result;
        }

        public Task<IQueryable<Judge>> QueryJudgesAsync(string? userId = null, int? groupId = 0, int? contestId = 0, int? problemId = 0, int? resultType = null)
        {
            IQueryable<Judge> judges = dbContext.Judge.AsNoTracking();
            if (!string.IsNullOrEmpty(userId)) judges = judges.Where(i => i.UserId == userId);
            if (groupId != 0) judges = judges.Where(i => i.GroupId == groupId);
            if (contestId != 0) judges = judges.Where(i => i.ContestId == contestId);
            if (problemId != 0) judges = judges.Where(i => i.ProblemId == problemId);
            if (resultType != null) judges = judges.Where(i => i.ResultType == resultType);

            return Task.FromResult(judges);
        }

        public async Task<int> QueueJudgeAsync(Judge judge)
        {
            judge.ResultType = (int)ResultCode.Pending;
            judge.JudgeCount++;
            var isRejudge = judge.Id != 0;
            if (isRejudge)
            {
                judge.Result = string.Empty;
                dbContext.Judge.Update(judge);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                judge.JudgeTime = DateTime.Now;
                await dbContext.Judge.AddAsync(judge);
                await dbContext.SaveChangesAsync();

                if (judge.ContestId != null)
                {
                    var problemConfig = await dbContext.ContestProblemConfig
                        .Where(i => i.ContestId == judge.ContestId &&
                                    i.ProblemId == judge.ProblemId).FirstOrDefaultAsync();
                    problemConfig.SubmissionCount++;
                }
                else
                {
                    var problem = await dbContext.Problem.Where(i => i.Id == judge.ProblemId).FirstOrDefaultAsync();
                    problem.SubmissionCount++;
                }
                await dbContext.SaveChangesAsync();
            }

            var (judgeOptionsBuilder, buildOptionsBuilder) = await JudgeHelper.GetOptionBuilders(problemService, judge, (await languageService.GetLanguageConfigAsync()).ToList());
            var (judgeOptions, buildOptions) = (judgeOptionsBuilder.Build(), buildOptionsBuilder.Build());

            var (channel, options) = messageQueueService.GetInstance("JudgeQueue");
            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;
            channel.BasicPublish(
                options.Exchange,
                options.RoutingKey,
                false,
                props,
                new JudgeInfo
                {
                    JudgeId = judge.Id,
                    Priority = isRejudge ? JudgePriority.Low : JudgePriority.Normal,
                    JudgeOptions = judgeOptions,
                    BuildOptions = buildOptions
                }.SerializeJson(false));
            return judge.Id;
        }

        public static ResultCode ComputeJudgeResultType(JudgeResult? result)
        {
            if (result?.JudgePoints == null)
            {
                return ResultCode.Unknown_Error;
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
        }

        public async Task UpdateJudgeResultAsync(int judgeId, JudgeReportInfo.ReportType reportType, JudgeResult? result)
        {
            var judge = await dbContext.Judge.FindAsync(judgeId);
            if (judge == null) return;

            if (reportType == JudgeReportInfo.ReportType.PostJudge)
            {
                judge.Result = result?.SerializeJsonAsString(false) ?? "{}";
                judge.ResultType = (int)ComputeJudgeResultType(result);
                judge.FullScore = result?.JudgePoints?.Sum(i => i.Score) ?? 0;
            }

            if (reportType == JudgeReportInfo.ReportType.PreJudge)
            {
                if (judge.ResultType == (int)ResultCode.Pending) judge.ResultType = (int)ResultCode.Judging;
            }

            dbContext.Judge.Update(judge);
            await dbContext.SaveChangesAsync();
        }
    }
}
