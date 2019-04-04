using System;
using System.Linq;
using System.Threading.Tasks;
using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hjudgeCore;
using hjudgeWebHost.Models.Problem;
using hjudgeWebHost.Services;
using hjudgeWebHost.Configurations;
using System.Collections.Generic;
using hjudgeWebHost.Utils;

namespace hjudgeWebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class ProblemController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IProblemService problemService;
        private readonly IJudgeService judgeService;
        private readonly ILanguageService languageService;
        private readonly ICacheService cacheService;
        private readonly ApplicationDbContext dbContext;

        public ProblemController(
            CachedUserManager<UserInfo> userManager,
            IProblemService problemService,
            IJudgeService judgeService,
            ILanguageService languageService,
            ICacheService cacheService,
            ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.problemService = problemService;
            this.judgeService = judgeService;
            this.languageService = languageService;
            this.cacheService = cacheService;
            this.dbContext = dbContext;
        }
        public class ProblemListQueryModel
        {
            public class ProblemFilter
            {
                public int Id { get; set; } = 0;
                public string Name { get; set; } = string.Empty;
                public int[] Status { get; set; } = new[] { 0, 1, 2 };
            }
            public int Start { get; set; }
            public int StartId { get; set; }
            public int Count { get; set; }
            public bool RequireTotalCount { get; set; }
            public int ContestId { get; set; }
            public int GroupId { get; set; }
            public ProblemFilter Filter { get; set; } = new ProblemFilter();
        }

        private readonly static int[] allStatus = new[] { 0, 1, 2 };

        [HttpPost]
        public async Task<ProblemListModel> ProblemList([FromBody]ProblemListQueryModel model)
        {
            var userId = userManager.GetUserId(User);

            var ret = new ProblemListModel();

            var judges = await judgeService.QueryJudgesAsync(
                userId,
                model.GroupId == 0 ? null : (int?)model.GroupId,
                model.ContestId == 0 ? null : (int?)model.ContestId,
                null);

            IQueryable<Problem> problems;

            try
            {
                problems = await (model switch
                {
                    { ContestId: 0, GroupId: 0 } => problemService.QueryProblemAsync(userId),
                    { GroupId: 0 } => problemService.QueryProblemAsync(userId, model.ContestId),
                    { } => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId)
                });
            }
            catch (Exception ex)
            {
                ret.ErrorCode = (ErrorDescription)ex.HResult;
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    ret.ErrorMessage = ex.Message;
                }
                return ret;
            }

            if (model.Filter.Id != 0)
            {
                problems = problems.Where(i => i.Id == model.Filter.Id);
            }
            if (!string.IsNullOrEmpty(model.Filter.Name))
            {
                problems = problems.Where(i => i.Name.Contains(model.Filter.Name));
            }

            if (model.Filter.Status.Length < 3)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    foreach (var status in allStatus)
                    {
                        if (!model.Filter.Status.Contains(status))
                        {
                            problems = status switch
                            {
                                0 => problems.Where(i => judges.Any(j => j.ProblemId == i.Id)),
                                1 => problems.Where(i => !judges.Any(j => j.ProblemId == i.Id && j.ResultType != (int)ResultCode.Accepted)),
                                2 => problems.Where(i => !judges.Any(j => j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted)),
                                _ => problems
                            };
                        }
                    }
                }
            }

            if (model.RequireTotalCount) ret.TotalCount = await problems.CountAsync();

            problems = problems.OrderBy(i => i.Id);
            if (model.StartId == 0) problems = problems.Skip(model.Start);
            else problems = problems.Where(i => i.Id >= model.StartId);

            if (model.ContestId != 0)
            {
                ret.Problems = await problems.Include(i => i.ContestProblemConfig).Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Level = i.Level,
                    AcceptCount = i.ContestProblemConfig.FirstOrDefault(j => j.ContestId == model.ContestId && j.ProblemId == i.Id).AcceptCount,
                    SubmissionCount = i.ContestProblemConfig.FirstOrDefault(j => j.ContestId == model.ContestId && j.ProblemId == i.Id).SubmissionCount,
                    Hidden = i.Hidden,
                    Upvote = i.Upvote,
                    Downvote = i.Downvote,
                    Status = judges.Any(j => j.ProblemId == i.Id) ?
                        (judges.Any(j => j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted) ? 2 : 1) : 0
                }).ToListAsync();
            }
            else
            {
                ret.Problems = await problems.Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Level = i.Level,
                    AcceptCount = i.AcceptCount,
                    SubmissionCount = i.SubmissionCount,
                    Hidden = i.Hidden,
                    Upvote = i.Upvote,
                    Downvote = i.Downvote,
                    Status = judges.Any(j => j.ProblemId == i.Id) ?
                        (judges.Any(j => j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted) ? 2 : 1) : 0
                }).ToListAsync();
            }

            return ret;
        }

        private IEnumerable<LanguageModel> GenerateLanguageConfig(IEnumerable<LanguageConfig> langConfig, string[]? languages)
        {
            foreach (var i in langConfig)
            {
                if (languages == null || languages.Length == 0 || languages.Contains(i.Name))
                {
                    yield return new LanguageModel
                    {
                        Name = i.Name,
                        Information = i.Information,
                        SyntaxHighlight = i.SyntaxHighlight
                    };
                }
            }
            yield break;
        }

        public class ProblemQueryModel
        {
            public int ProblemId { get; set; }
            public int ContestId { get; set; }
            public int GroupId { get; set; }
        }

        [HttpPost]
        public async Task<ProblemModel> ProblemDetails([FromBody]ProblemQueryModel model)
        {
            var userId = userManager.GetUserId(User);
            var ret = new ProblemModel();

            IQueryable<Problem> problems;
            try
            {
                problems = await (model switch
                {
                    { ContestId: 0, GroupId: 0 } => problemService.QueryProblemAsync(userId),
                    { GroupId: 0 } => problemService.QueryProblemAsync(userId, model.ContestId),
                    { } => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId)
                });
            }
            catch (Exception ex)
            {
                ret.ErrorCode = (ErrorDescription)ex.HResult;
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    ret.ErrorMessage = ex.Message;
                }
                return ret;
            }

            Problem? problem = default;
            if (await problems.AnyAsync(i => i.Id == model.ProblemId))
            {
                problem = await problemService.GetProblemAsync(model.ProblemId);
            }
            if (problem == null)
            {
                ret.ErrorCode = ErrorDescription.ResourceNotFound;
                return ret;
            }

            var judges = await judgeService.QueryJudgesAsync(
                userId,
                model.GroupId == 0 ? null : (int?)model.GroupId,
                model.ContestId == 0 ? null : (int?)model.ContestId,
                null);

            if (judges.Any(i => i.ProblemId == problem.Id))
            {
                ret.Status = 1;
                if (judges.Any(i => i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
                {
                    ret.Status = 2;
                }
            }

            ret.AcceptCount = problem.AcceptCount;
            ret.SubmissionCount = problem.SubmissionCount;

            if (model.ContestId != 0)
            {
                var data = await dbContext.ContestProblemConfig.Where(i => i.ContestId == model.ContestId && i.ProblemId == problem.Id).Select(i => new { i.AcceptCount, i.SubmissionCount }).FirstOrDefaultAsync();
                if (data != null)
                {
                    ret.AcceptCount = data.AcceptCount;
                    ret.SubmissionCount = data.SubmissionCount;
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                if (judges.Any(i => i.ProblemId == problem.Id))
                {
                    ret.Status = 1;
                    if (judges.Any(i => i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
                    {
                        ret.Status = 2;
                    }
                }
            }

            var user = await cacheService.GetObjectAndSetAsync($"user_{problem.UserId}", () => userManager.FindByIdAsync(problem.UserId));
            ret.Name = problem.Name;
            ret.Hidden = problem.Hidden;
            ret.Level = problem.Level;
            ret.Type = problem.Type;
            ret.UserId = problem.UserId;
            ret.UserName = user?.UserName;
            ret.Id = problem.Id;
            ret.Description = problem.Description;
            ret.CreationTime = problem.CreationTime;
            ret.Upvote = problem.Upvote;
            ret.Downvote = problem.Downvote;

            var config = problem.Config.DeserializeJson<ProblemConfig>(false);

            var langConfig = await languageService.GetLanguageConfigAsync();
            var langs = config?.Languages?.Split(';', StringSplitOptions.RemoveEmptyEntries);

            ret.Languages = GenerateLanguageConfig(langConfig, langs).ToArray();

            return ret;
        }
    }
}