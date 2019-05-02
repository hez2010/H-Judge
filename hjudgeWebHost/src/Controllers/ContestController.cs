using hjudgeWebHost.Configurations;
using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Models.Contest;
using hjudgeWebHost.Services;
using hjudgeShared.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;

namespace hjudgeWebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class ContestController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IContestService contestService;

        public ContestController(
            CachedUserManager<UserInfo> userManager,
            IContestService contestService)
        {
            this.userManager = userManager;
            this.contestService = contestService;
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
            public int StartId { get; set; }
            public int Count { get; set; }
            public bool RequireTotalCount { get; set; }
            public int GroupId { get; set; }
            public ContestFilter Filter { get; set; } = new ContestFilter();
        }

        [HttpPost]
        public async Task<ContestListModel> ContestList([FromBody]ContestListQueryModel model)
        {
            var userId = userManager.GetUserId(User);

            var ret = new ContestListModel();

            IQueryable<Contest> contests;

            try
            {
                contests = await (model.GroupId switch
                {
                    0 => contestService.QueryContestAsync(userId),
                    _ => contestService.QueryContestAsync(userId, model.GroupId)
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

            if (model.RequireTotalCount) ret.TotalCount = await contests.CountAsync();

            contests = contests.OrderByDescending(i => i.Id);

            if (model.StartId == 0) contests = contests.Skip(model.Start);
            else contests = contests.Where(i => i.Id <= model.StartId);

            ret.Contests = await contests.OrderByDescending(i => i.Id).Skip(model.Start).Take(model.Count).Select(i => new ContestListModel.ContestListItemModel
            {
                Id = i.Id,
                Downvote = i.Downvote,
                EndTime = i.EndTime,
                Hidden = i.Hidden,
                Name = i.Name,
                StartTime = i.StartTime,
                Upvote = i.Upvote,
                Status = now < i.StartTime ? 0 :
                    now > i.EndTime ? 2 : 1
            }).Cacheable().ToListAsync();

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

            var ret = new ContestModel();

            IQueryable<Contest> contests;

            try
            {
                contests = await (model switch
                {
                    { GroupId: 0 } => contestService.QueryContestAsync(userId),
                    { } => contestService.QueryContestAsync(userId, model.GroupId)
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

            var contest = await contests.Include(i => i.UserInfo).Where(i => i.Id == model.ContestId).Cacheable().FirstOrDefaultAsync();

            if (contest == null)
            {
                ret.ErrorCode = ErrorDescription.ResourceNotFound;
                return ret;
            }

            var user = await userManager.FindByIdAsync(contest.UserId);
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
            ret.Config = contest.Config.DeserializeJson<ContestConfig>(false);

            return ret;
        }
    }
}
