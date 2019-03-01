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

namespace hjudgeWebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class ProblemController : ControllerBase
    {
        private readonly DbContextOptions<ApplicationDbContext> DbOptions;
        private readonly UserManager<UserInfo> UserManager;
        public ProblemController(DbContextOptions<ApplicationDbContext> dbOptions, UserManager<UserInfo> userManager)
        {
            DbOptions = dbOptions;
            UserManager = userManager;
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
            public ProblemFilter Filter { get; set; } = new ProblemFilter();
        }

        [HttpPost]
        public async Task<ProblemListModel> ProblemList([FromBody]ProblemListQueryModel model)
        {
            var user = await UserManager.GetUserAsync(User);
            using var db = new ApplicationDbContext(DbOptions);
            var ret = new ProblemListModel();
            IQueryable<Problem> query = db.Problem;
            if (model.Filter.Id != 0)
            {
                query = query.Where(i => i.Id == model.Filter.Id);
            }
            if (!string.IsNullOrEmpty(model.Filter.Name))
            {
                query = query.Where(i => i.Name.Contains(model.Filter.Name));
            }
            if (model.Filter.Status.Length < 3)
            {
                if (user != null)
                {
                    foreach (var status in new[] { 0, 1, 2 })
                    {
                        if (!model.Filter.Status.Contains(status))
                        {
                            query = status switch
                            {
                                0 => query.Where(i => db.Judge.Any(j => j.UserId == user.Id && j.ProblemId == i.Id)),
                                1 => query.Where(i => !db.Judge.Any(j => j.UserId == user.Id && j.ProblemId == i.Id && j.ResultType != (int)ResultCode.Accepted)),
                                2 => query.Where(i => !db.Judge.Any(j => j.UserId == user.Id && j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted)),
                                _ => query
                            };
                        }
                    }
                }
            }
            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege ?? 0))
            {
                query = query.Where(i => !i.Hidden);
            }
            ret.Problems = await query.Skip(model.Start).Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
            {
                Id = i.Id,
                Name = i.Name,
                Level = i.Level,
                AcceptCount = i.AcceptCount,
                SubmissionCount = i.SubmissionCount
            }).OrderBy(i => i.Id).ToListAsync();
            if (model.RequireTotalCount)
                ret.TotalCount = await query.CountAsync();
            if (user != null)
            {
                foreach (var problem in ret.Problems)
                {
                    if (db.Judge.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id))
                    {
                        problem.Status = 1;
                        if (db.Judge.Any(i => i.UserId == user.Id && i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted))
                        {
                            problem.Status = 2;
                        }
                    }
                }
            }
            return ret;
        }
    }
}