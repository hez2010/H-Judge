using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Models.Contest;
using hjudgeWebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
    public class ContestController : ControllerBase
    {
        private readonly DbContextOptions<ApplicationDbContext> DbOptions;
        private readonly UserManager<UserInfo> UserManager;
        private readonly IContestService ContestService;
        public ContestController(DbContextOptions<ApplicationDbContext> dbOptions, UserManager<UserInfo> userManager, IContestService contestService)
        {
            DbOptions = dbOptions;
            UserManager = userManager;
            ContestService = contestService;
        }

        public class ContestListQueryModel
        {
            public class ContestFilter
            {
                public int Id { get; set; } = 0;
                public string Name { get; set; } = string.Empty;
                public int[] Status { get; set; } = new[] { 0, 1, 2 };
            }
            public int Start { get; set; }
            public int Count { get; set; }
            public bool RequireTotalCount { get; set; }
            public int GroupId { get; set; }
            public ContestFilter Filter { get; set; } = new ContestFilter();
        }

        [HttpPost]
        public async Task<ContestListModel> ContestList([FromBody]ContestListQueryModel model)
        {
            var user = await UserManager.GetUserAsync(User);
            using var db = new ApplicationDbContext(DbOptions);

            var (_, contests) = model.GroupId switch
            {
                0 => ContestService.QueryContest(db),
                _ => ContestService.QueryContest(model.GroupId, db)
            };

            var ret = new ContestListModel
            {
                Succeeded = false,
                ErrorCode = ErrorDescription.ResourceNotFound
            };

            var group = await db.Group.FirstOrDefaultAsync(i => i.Id == model.GroupId);
            if (model.GroupId != 0 && group == null) return ret;

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (model.GroupId != 0 && group.IsPrivate)
                {
                    if (user == null ||
                        !db.GroupJoin
                            .Any(i => i.GroupId == model.GroupId &&
                                i.UserId == user.Id))
                        return ret;
                }

                if (model.GroupId == 0) contests = contests.Where(i => !i.Hidden);
            }

            if (model.Filter.Id != 0)
            {
                contests = contests.Where(i => i.Id == model.Filter.Id);
            }
            if (!string.IsNullOrEmpty(model.Filter.Name))
            {
                contests = contests.Where(i => i.Name.Contains(model.Filter.Name));
            }
            var now = DateTime.Now;

            if (model.Filter.Status.Length < 3)
            {
                if (user != null)
                {
                    foreach (var status in new[] { 0, 1, 2 })
                    {
                        if (!model.Filter.Status.Contains(status))
                        {
                            contests = status switch
                            {
                                0 => contests.Where(i => !(now < i.StartTime)),
                                1 => contests.Where(i => !(i.StartTime >= now && i.EndTime <= now)),
                                2 => contests.Where(i => !(now > i.EndTime)),
                                _ => contests
                            };
                        }
                    }
                }
            }

            ret.Contests = await contests.OrderByDescending(i => i.Id).Skip(model.Start).Take(model.Count).Select(i => new ContestListModel.ContestListItemModel
            {
                Id = i.Id,
                Downvote = i.Downvote,
                EndTime = i.EndTime,
                Hidden = i.Hidden,
                Name = i.Name,
                StartTime = i.StartTime,
                Upvote = i.Upvote
            }).ToListAsync();
            if (model.RequireTotalCount) ret.TotalCount = await contests.CountAsync();

            foreach (var contest in ret.Contests)
            {
                contest.Status = now < contest.StartTime ? 0 :
                    now > contest.EndTime ? 2 : 1;
            }

            ret.Succeeded = true;
            return ret;
        }
    }
}
