using hjudgeWebHost.Configurations;
using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Models.Contest;
using hjudgeWebHost.Services;
using hjudgeWebHost.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
    public class ContestController : ControllerBase
    {
        private readonly DbContextOptions<ApplicationDbContext> dbOptions;
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IContestService contestService;
        private readonly ICacheService cacheService;

        public ContestController(
            DbContextOptions<ApplicationDbContext> dbOptions,
            CachedUserManager<UserInfo> userManager,
            IContestService contestService,
            ICacheService cacheService)
        {
            this.dbOptions = dbOptions;
            this.userManager = userManager;
            this.contestService = contestService;
            this.cacheService = cacheService;
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
            var userId = userManager.GetUserId(User);
            using var db = new ApplicationDbContext(dbOptions);

            var ret = new ContestListModel();

            IQueryable<Contest> contests;

            try
            {
                contests = await (model.GroupId switch
                {
                    0 => contestService.QueryContestAsync(userId, db),
                    _ => contestService.QueryContestAsync(userId, model.GroupId, db)
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
                contests = contests.Where(i => i.Id == model.Filter.Id);
            }
            if (!string.IsNullOrEmpty(model.Filter.Name))
            {
                contests = contests.Where(i => i.Name.Contains(model.Filter.Name));
            }
            var now = DateTime.Now;

            if (model.Filter.Status.Length < 3)
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

            return ret;
        }

        public class ContestQueryModel
        {
            public int ContestId { get; set; }
            public int GroupId { get; set; }
        }

        [HttpPost]
        public async Task<ContestModel> ContestDetails([FromBody]ContestQueryModel model)
        {
            var userId = userManager.GetUserId(User);

            using var db = new ApplicationDbContext(dbOptions);

            var ret = new ContestModel();

            IQueryable<Contest> contests;

            try
            {
                contests = await (model switch
                {
                    { GroupId: 0 } => contestService.QueryContestAsync(userId, db),
                    { } => contestService.QueryContestAsync(userId, model.GroupId, db)
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

            var contest = await contests.Include(i => i.UserInfo).FirstOrDefaultAsync(i => i.Id == model.ContestId);

            var user = await cacheService.GetObjectAndSetAsync($"user_{contest.UserId}", () => userManager.FindByIdAsync(contest.UserId));
            ret.Description = contest.Description;
            ret.Downvote = contest.Downvote;
            ret.EndTime = contest.EndTime;
            ret.Hidden = contest.Hidden;
            ret.Id = contest.Id;
            ret.Name = contest.Name;
            ret.Password = contest.Password;
            ret.StartTime = contest.StartTime;
            ret.Upvote = contest.Upvote;
            ret.UserId = contest.UserId;
            ret.UserName = contest.UserInfo.UserName;
            ret.Config = contest.Config.DeserializeJsonString<ContestConfig>();

            return ret;
        }
    }
}
