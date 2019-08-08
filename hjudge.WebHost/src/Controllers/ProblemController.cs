using System;
using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hjudge.Core;
using hjudge.WebHost.Models.Problem;
using hjudge.WebHost.Services;
using hjudge.WebHost.Configurations;
using hjudge.Shared.Utils;
using EFSecondLevelCache.Core;
using hjudge.WebHost.Utils;
using hjudge.WebHost.Models;
using static hjudge.WebHost.Middlewares.PrivilegeAuthentication;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("problem")]
    [ApiController]
    public class ProblemController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IProblemService problemService;
        private readonly IJudgeService judgeService;
        private readonly ILanguageService languageService;
        private readonly WebHostDbContext dbContext;

        public ProblemController(
            CachedUserManager<UserInfo> userManager,
            IProblemService problemService,
            IJudgeService judgeService,
            ILanguageService languageService,
            WebHostDbContext dbContext)
        {
            this.userManager = userManager;
            this.problemService = problemService;
            this.judgeService = judgeService;
            this.languageService = languageService;
            this.dbContext = dbContext;
        }

        private readonly static int[] allStatus = new[] { 0, 1, 2 };

        [HttpPost]
        [Route("list")]
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

            if (model.Filter.Status.Count < 3)
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

            if (model.RequireTotalCount) ret.TotalCount = await problems.Select(i => i.Id)/*.Cacheable()*/.CountAsync();

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
                    Hidden = false,
                    Upvote = i.Upvote,
                    Downvote = i.Downvote,
                    Status = judges.Any(j => j.ProblemId == i.Id) ?
                        (judges.Any(j => j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted) ? 2 : 1) : 0
                })/*.Cacheable()*/.ToListAsync();
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
                })/*.Cacheable()*/.ToListAsync();
            }

            return ret;
        }


        [HttpPost]
        [Route("details")]
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

            var problem = await problems.Where(i => i.Id == model.ProblemId)/*.Cacheable()*/.FirstOrDefaultAsync();
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

            if (await judges.Where(i => i.ProblemId == problem.Id)
                            /*.Cacheable()*/.AnyAsync())
            {
                ret.Status = 1;
                if (await judges.Where(i => i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted)
                                /*.Cacheable()*/.AnyAsync())
                {
                    ret.Status = 2;
                }
            }

            ret.AcceptCount = problem.AcceptCount;
            ret.SubmissionCount = problem.SubmissionCount;

            if (model.ContestId != 0)
            {
                var data = await dbContext.ContestProblemConfig
                    .Where(i => i.ContestId == model.ContestId && i.ProblemId == problem.Id)
                    .Select(i => new { i.AcceptCount, i.SubmissionCount })
                    /*.Cacheable()*/
                    .FirstOrDefaultAsync();
                if (data != null)
                {
                    ret.AcceptCount = data.AcceptCount;
                    ret.SubmissionCount = data.SubmissionCount;
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                if (await judges.Where(i => i.ProblemId == problem.Id)/*.Cacheable()*/.AnyAsync())
                {
                    ret.Status = 1;
                    if (await judges.Where(i => i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted)
                                    /*.Cacheable()*/.AnyAsync())
                    {
                        ret.Status = 2;
                    }
                }
            }

            var user = await userManager.FindByIdAsync(problem.UserId);
            ret.Name = problem.Name;
            ret.Hidden = problem.Hidden;
            ret.Level = problem.Level;
            ret.Type = problem.Type;
            ret.UserId = problem.UserId;
            ret.UserName = user?.UserName ?? string.Empty;
            ret.Id = problem.Id;
            ret.Description = problem.Description;
            ret.CreationTime = problem.CreationTime;
            ret.Upvote = problem.Upvote;
            ret.Downvote = problem.Downvote;

            var config = problem.Config.DeserializeJson<ProblemConfig>(false);

            var langConfig = await languageService.GetLanguageConfigAsync();
            var langs = config?.Languages?.Split(';', StringSplitOptions.RemoveEmptyEntries);

            ret.Languages = LanguageConfigHelper.GenerateLanguageConfig(langConfig, langs).ToList();

            return ret;
        }

        [HttpDelete]
        [RequireAdmin]
        [Route("edit")]
        public async Task<ResultModel> RemoveProblem(int problemId)
        {
            var ret = new ResultModel();

            await problemService.RemoveProblemAsync(problemId);
            return ret;
        }

        [HttpPut]
        [RequireAdmin]
        [Route("edit")]
        public async Task<ProblemEditModel> CreateProblem([FromBody]ProblemEditModel model)
        {
            var userId = userManager.GetUserId(User);
            var ret = new ProblemEditModel();

            var problem = new Problem
            {
                Description = model.Description,
                Hidden = model.Hidden,
                Level = model.Level,
                Name = model.Name,
                Type = model.Type,
                CreationTime = DateTime.Now,
                UserId = userId,
                Config = model.Config.SerializeJsonAsString(false)
            };

            problem.Id = await problemService.CreateProblemAsync(problem);

            ret.Description = problem.Description;
            ret.Hidden = problem.Hidden;
            ret.Id = problem.Id;
            ret.Level = problem.Level;
            ret.Name = problem.Name;
            ret.Type = problem.Type;
            ret.Config = problem.Config.DeserializeJson<ProblemConfig>(false);

            return ret;
        }

        [HttpPost]
        [RequireAdmin]
        [Route("edit")]
        public async Task<ResultModel> UpdateProblem([FromBody]ProblemEditModel model)
        {
            var ret = new ResultModel();

            var problem = await problemService.GetProblemAsync(model.Id);
            if (problem == null)
            {
                ret.ErrorCode = ErrorDescription.ResourceNotFound;
                return ret;
            }

            problem.Description = model.Description;
            problem.Hidden = model.Hidden;
            problem.Level = model.Level;
            problem.Name = model.Name;
            problem.Type = model.Type;
            problem.Config = model.Config.SerializeJsonAsString(false);

            await problemService.UpdateProblemAsync(problem);

            return ret;
        }

        [HttpGet]
        [RequireAdmin]
        [Route("edit")]
        public async Task<ProblemEditModel> GetProblem(int problemId)
        {
            var ret = new ProblemEditModel();

            var problem = await problemService.GetProblemAsync(problemId);
            if (problem == null)
            {
                ret.ErrorCode = ErrorDescription.ResourceNotFound;
                return ret;
            }

            ret.Description = problem.Description;
            ret.Hidden = problem.Hidden;
            ret.Id = problem.Id;
            ret.Level = problem.Level;
            ret.Name = problem.Name;
            ret.Type = problem.Type;
            ret.Config = problem.Config.DeserializeJson<ProblemConfig>(false);

            return ret;
        }
    }
}