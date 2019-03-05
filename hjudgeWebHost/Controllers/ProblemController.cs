using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hjudgeCore;
using hjudgeWebHost.Models.Problem;
using Newtonsoft.Json.Linq;
using hjudgeWebHost.Services;

namespace hjudgeWebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class ProblemController : ControllerBase
    {
        private readonly DbContextOptions<ApplicationDbContext> DbOptions;
        private readonly UserManager<UserInfo> UserManager;
        private readonly IProblemService ProblemService;
        public ProblemController(DbContextOptions<ApplicationDbContext> dbOptions, UserManager<UserInfo> userManager, IProblemService problemService)
        {
            DbOptions = dbOptions;
            UserManager = userManager;
            ProblemService = problemService;
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

        [HttpPost]
        public async Task<ProblemListModel> ProblemList([FromBody]ProblemListQueryModel model)
        {
            var user = await UserManager.GetUserAsync(User);
            using var db = new ApplicationDbContext(DbOptions);

            var (_, problems) = model switch
            {
                { ContestId: 0, GroupId: 0 } => ProblemService.QueryProblem(db),
                { GroupId: 0 } => ProblemService.QueryProblem(model.ContestId, db),
                { } => ProblemService.QueryProblem(model.ContestId, model.GroupId, db)
            };

            var ret = new ProblemListModel
            {
                ErrorCode = ErrorDescription.ResourceNotFound
            };

            var contest = await db.Contest.FirstOrDefaultAsync(i => i.Id == model.ContestId);
            if (model.ContestId != 0 && contest == null) return ret;

            var group = await db.Group.FirstOrDefaultAsync(i => i.Id == model.GroupId);
            if (model.GroupId != 0)
            {
                if (group == null) return ret;
                if (!db.GroupContestConfig.Any(i => i.GroupId == model.GroupId && i.ContestId == model.ContestId)) return ret;
            }

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (model.GroupId != 0 && group.IsPrivate)
                {
                    if (user == null ||
                        !db.GroupJoin.Any(i => i.GroupId == model.GroupId && i.UserId == user.Id))
                        return ret;
                }

                if (model.ContestId == 0) problems = problems.Where(i => !i.Hidden);
                else if (contest.Hidden) return ret;
            }

            if (model.Filter.Id != 0)
            {
                problems = problems.Where(i => i.Id == model.Filter.Id);
            }
            if (!string.IsNullOrEmpty(model.Filter.Name))
            {
                problems = problems.Where(i => i.Name.Contains(model.Filter.Name));
            }

            IQueryable<Judge> judges = db.Judge;
            judges = model.ContestId != 0 ? judges.Where(i => i.ContestId == model.ContestId) : judges.Where(i => i.ContestId == null);
            judges = model.GroupId != 0 ? judges.Where(i => i.GroupId == model.GroupId) : judges.Where(i => i.GroupId == null);

            if (model.Filter.Status.Length < 3)
            {
                if (user != null)
                {
                    foreach (var status in new[] { 0, 1, 2 })
                    {
                        if (!model.Filter.Status.Contains(status))
                        {
                            problems = status switch
                            {
                                0 => problems.Where(i => judges.Any(j => j.UserId == user.Id && j.ProblemId == i.Id)),
                                1 => problems.Where(i => !judges.Any(j => j.UserId == user.Id && j.ProblemId == i.Id && j.ResultType != (int)ResultCode.Accepted)),
                                2 => problems.Where(i => !judges.Any(j => j.UserId == user.Id && j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted)),
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

            if (user != null)
            {
                foreach (var problem in ret.Problems)
                {
                    if (judges.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id))
                    {
                        problem.Status = 1;
                        if (judges.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
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
            var user = await UserManager.GetUserAsync(User);
            using var db = new ApplicationDbContext(DbOptions);
            var ret = new ProblemModel();

            IQueryable<Problem> problems = db.Problem;
            IQueryable<ContestProblemConfig> contestConfigs = db.ContestProblemConfig;
            if (model.GroupId != 0)
            {
                var groups = db.GroupContestConfig.Where(i => i.GroupId == model.GroupId);
                contestConfigs = contestConfigs.Where(i => groups.Any(j => j.ContestId == i.ContestId));
            }

            if (model.ContestId != 0)
            {
                problems = contestConfigs.Include(i => i.Problem).Where(i => i.ContestId == model.ContestId).Select(i => i.Problem);
            }
            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege ?? 0))
            {
                problems = problems.Where(i => !i.Hidden);
            }

            var problem = await problems.Include(i => i.UserInfo).FirstOrDefaultAsync(i => i.Id == model.ProblemId);

            IQueryable<Judge> judges = db.Judge;
            judges = model.ContestId != 0 ? judges.Where(i => i.ContestId == model.ContestId) : judges.Where(i => i.ContestId == null);
            judges = model.GroupId != 0 ? judges.Where(i => i.GroupId == model.GroupId) : judges.Where(i => i.GroupId == null);

            if (judges.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id))
            {
                ret.Status = 1;
                if (judges.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
                {
                    ret.Status = 2;
                }
            }

            if (model.ContestId != 0)
            {
                var data = await contestConfigs.Where(i => i.ContestId == model.ContestId && i.ProblemId == model.ProblemId).Select(i => new { i.ProblemId, i.AcceptCount, i.SubmissionCount }).FirstOrDefaultAsync();
                if (data != null)
                {
                    ret.AcceptCount = data.AcceptCount;
                    ret.SubmissionCount = data.SubmissionCount;
                }
            }
            else
            {
                ret.AcceptCount = problem.AcceptCount;
                ret.SubmissionCount = problem.SubmissionCount;
            }

            if (user != null)
            {
                if (judges.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id))
                {
                    ret.Status = 1;
                    if (judges.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
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
            var config = JObject.Parse(problem.Config);
            if (config["Languages"] != null)
            {
                ret.Languages = config["Languages"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);
            }

            return ret;
        }
    }
}