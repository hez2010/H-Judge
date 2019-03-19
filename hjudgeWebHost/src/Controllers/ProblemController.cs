using System;
using System.Linq;
using System.Threading.Tasks;
using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hjudgeCore;
using hjudgeWebHost.Models.Problem;
using hjudgeWebHost.Services;
using System.Text;
using hjudgeWebHost.Configurations;

namespace hjudgeWebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class ProblemController : ControllerBase
    {
        private readonly DbContextOptions<ApplicationDbContext> dbOptions;
        private readonly UserManager<UserInfo> userManager;
        private readonly IProblemService problemService;
        private readonly IJudgeService judgeService;

        public ProblemController(DbContextOptions<ApplicationDbContext> dbOptions, UserManager<UserInfo> userManager, IProblemService problemService, IJudgeService judgeService)
        {
            this.dbOptions = dbOptions;
            this.userManager = userManager;
            this.problemService = problemService;
            this.judgeService = judgeService;
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
            using var db = new ApplicationDbContext(dbOptions);

            var ret = new ProblemListModel
            {
                ErrorCode = ErrorDescription.ResourceNotFound
            };

            IQueryable<Problem> problems;

            try
            {
                problems = await (model switch
                {
                    { ContestId: 0, GroupId: 0 } => problemService.QueryProblemAsync(userId, db),
                    { GroupId: 0 } => problemService.QueryProblemAsync(userId, model.ContestId, db),
                    { } => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId, db)
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

            var judges = await judgeService.QueryJudgesAsync(
                userId,
                model.GroupId == 0 ? null : (int?)model.GroupId,
                model.ContestId == 0 ? null : (int?)model.ContestId,
                null,
                db);

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
                                0 => problems.Where(i => judges.Any(j => j.UserId == userId && j.ProblemId == i.Id)),
                                1 => problems.Where(i => !judges.Any(j => j.UserId == userId && j.ProblemId == i.Id && j.ResultType != (int)ResultCode.Accepted)),
                                2 => problems.Where(i => !judges.Any(j => j.UserId == userId && j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted)),
                                _ => problems
                            };
                        }
                    }
                }
            }

            ret.Problems = await problems.OrderBy(i => i.Id).Skip(model.Start).Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
            {
                Id = i.Id,
                Name = i.Name,
                Level = i.Level,
                AcceptCount = i.AcceptCount,
                SubmissionCount = i.SubmissionCount,
                Hidden = i.Hidden,
                Upvote = i.Upvote,
                Downvote = i.Downvote
            }).ToListAsync();

            if (model.RequireTotalCount) ret.TotalCount = await problems.CountAsync();

            if (model.ContestId != 0)
            {
                foreach (var problem in ret.Problems)
                {
                    var data = await db.ContestProblemConfig.Where(i => i.ContestId == model.ContestId && i.ProblemId == problem.Id).Select(i => new { i.AcceptCount, i.SubmissionCount }).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        problem.AcceptCount = data.AcceptCount;
                        problem.SubmissionCount = data.SubmissionCount;
                    }
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                foreach (var problem in ret.Problems)
                {
                    if (judges.Any(i => i.UserId == userId && i.ProblemId == problem.Id))
                    {
                        problem.Status = 1;
                        if (judges.Any(i => i.UserId == userId && i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
                        {
                            problem.Status = 2;
                        }
                    }
                }
            }

            ret.Succeeded = true;
            return ret;
        }

        public class ProblemQueryModel
        {
            public int ProblemId { get; set; }
            public int ContestId { get; set; }
            public int GroupId { get; set; }
        }

        [HttpPost]
        public async Task<ProblemModel> ProblemDetail([FromBody]ProblemQueryModel model)
        {
            var userId = userManager.GetUserId(User);

            using var db = new ApplicationDbContext(dbOptions);

            var ret = new ProblemModel();

            IQueryable<Problem> problems;
            try
            {
                problems = await (model switch
                {
                    { ContestId: 0, GroupId: 0 } => problemService.QueryProblemAsync(userId, db),
                    { GroupId: 0 } => problemService.QueryProblemAsync(userId, model.ContestId, db),
                    { } => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId, db)
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

            var problem = await problems.Include(i => i.UserInfo).FirstOrDefaultAsync(i => i.Id == model.ProblemId);

            IQueryable<Judge> judges = db.Judge;
            judges = model.ContestId != 0 ? judges.Where(i => i.ContestId == model.ContestId) : judges.Where(i => i.ContestId == null);
            judges = model.GroupId != 0 ? judges.Where(i => i.GroupId == model.GroupId) : judges.Where(i => i.GroupId == null);

            if (judges.Any(i => i.UserId == userId && i.ProblemId == problem.Id))
            {
                ret.Status = 1;
                if (judges.Any(i => i.UserId == userId && i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
                {
                    ret.Status = 2;
                }
            }

            ret.AcceptCount = problem.AcceptCount;
            ret.SubmissionCount = problem.SubmissionCount;

            if (model.ContestId != 0)
            {
                var data = await db.ContestProblemConfig.Where(i => i.ContestId == model.ContestId && i.ProblemId == problem.Id).Select(i => new { i.AcceptCount, i.SubmissionCount }).FirstOrDefaultAsync();
                if (data != null)
                {
                    ret.AcceptCount = data.AcceptCount;
                    ret.SubmissionCount = data.SubmissionCount;
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                if (judges.Any(i => i.UserId == userId && i.ProblemId == problem.Id))
                {
                    ret.Status = 1;
                    if (judges.Any(i => i.UserId == userId && i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
                    {
                        ret.Status = 2;
                    }
                }
            }

            ret.Name = problem.Name;
            ret.Hidden = problem.Hidden;
            ret.Level = problem.Level;
            ret.Type = problem.Type;
            ret.UserId = problem.UserId;
            ret.UserName = problem.UserInfo.UserName;
            ret.Id = problem.Id;
            ret.Description = problem.Description;
            ret.CreationTime = problem.CreationTime;
            ret.Upvote = problem.Upvote;
            ret.Downvote = problem.Downvote;

            var config = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<ProblemConfig>(Encoding.UTF8.GetBytes(problem.Config).AsSpan());
            if (config?.Languages != null)
            {
                ret.Languages = config.Languages.ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);
            }

            return ret;
        }
    }
}