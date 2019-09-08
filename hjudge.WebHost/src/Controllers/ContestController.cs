using hjudge.WebHost.Configurations;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Models.Contest;
using hjudge.WebHost.Services;
using hjudge.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using static hjudge.WebHost.Middlewares.PrivilegeAuthentication;
using hjudge.WebHost.Exceptions;
using System.Net;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("contest")]
    [ApiController]
    public class ContestController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IContestService contestService;
        private readonly IProblemService problemService;
        private readonly IVoteService voteService;

        public ContestController(
            CachedUserManager<UserInfo> userManager,
            IContestService contestService,
            IProblemService problemService,
            IVoteService voteService)
        {
            this.userManager = userManager;
            this.contestService = contestService;
            this.problemService = problemService;
            this.voteService = voteService;
        }

        private static readonly int[] allStatus = { 0, 1, 2 };
        [HttpPost]
        [Route("list")]
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
                throw new InterfaceException((HttpStatusCode)ex.HResult, ex.Message);
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

            if (model.Filter.Status.Count < 3)
            {
                foreach (var status in allStatus)
                {
                    if (!model.Filter.Status.Contains(status))
                    {
                        contests = status switch
                        {
                            0 => contests.Where(i => !(now < i.StartTime)),
                            1 => contests.Where(i => !(i.StartTime <= now && i.EndTime > now)),
                            2 => contests.Where(i => !(now >= i.EndTime)),
                            _ => contests
                        };
                    }
                }
            }

            if (model.RequireTotalCount) ret.TotalCount = await contests.Select(i => i.Id)/*.Cacheable()*/.CountAsync();

            if (model.GroupId == 0) contests = contests.OrderByDescending(i => i.Id);
            else model.StartId = 0; // keep original order while fetching contests in a group

            if (model.StartId == 0) contests = contests.Skip(model.Start);
            else contests = contests.Where(i => i.Id <= model.StartId);

            ret.Contests = await contests.Take(model.Count).Select(i => new ContestListModel.ContestListItemModel
            {
                Id = i.Id,
                Downvote = i.Downvote,
                EndTime = i.EndTime,
                Hidden = i.Hidden,
                Name = i.Name,
                StartTime = i.StartTime,
                Upvote = i.Upvote
            })/*.Cacheable()*/.ToListAsync();

            ret.CurrentTime = DateTime.Now;
            return ret;
        }

        [HttpPost]
        [Route("details")]
        public async Task<ContestModel> ContestDetails([FromBody]ContestQueryModel model)
        {
            var userId = userManager.GetUserId(User);


            IQueryable<Contest> contests;

            try
            {
                contests = await (model switch
                {
                    { GroupId: 0 } => contestService.QueryContestAsync(userId),
                    _ => contestService.QueryContestAsync(userId, model.GroupId)
                });
            }
            catch (Exception ex)
            {
                throw new InterfaceException((HttpStatusCode)ex.HResult, ex.Message);
            }

            var contest = await contests.Include(i => i.UserInfo).Where(i => i.Id == model.ContestId)/*.Cacheable()*/.FirstOrDefaultAsync();

            if (contest == null) throw new NotFoundException("找不到该比赛");

            var vote = await voteService.GetVoteAsync(userId, null, model.ContestId);

            return new ContestModel
            {
                Description = contest.Description,
                Downvote = contest.Downvote,
                EndTime = contest.EndTime,
                Hidden = contest.Hidden,
                Id = contest.Id,
                Name = contest.Name,
                Password = contest.Password,
                StartTime = contest.StartTime,
                Upvote = contest.Upvote,
                UserId = contest.UserId,
                UserName = contest.UserInfo.UserName,
                Config = contest.Config.DeserializeJson<ContestConfig>(false),
                CurrentTime = DateTime.Now,
                MyVote = vote?.VoteType ?? 0
            };
        }

        [HttpDelete]
        [RequireTeacher]
        [Route("edit")]
        public Task RemoveContest(int contestId)
        {
            return contestService.RemoveContestAsync(contestId);
        }

        [HttpPut]
        [RequireTeacher]
        [Route("edit")]
        public async Task<ContestEditModel> CreateContest([FromBody]ContestEditModel model)
        {
            var userId = userManager.GetUserId(User);
            var ret = new ContestEditModel();

            var contest = new Contest
            {
                Description = model.Description,
                Hidden = model.Hidden,
                UserId = userId,
                Name = model.Name,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Password = model.Password,
                Config = model.Config.SerializeJsonAsString(false)
            };

            contest.Id = await contestService.CreateContestAsync(contest);
            await contestService.UpdateContestProblemAsync(contest.Id, model.Problems);

            ret.Description = contest.Description;
            ret.Hidden = contest.Hidden;
            ret.Id = contest.Id;
            ret.StartTime = contest.StartTime;
            ret.EndTime = contest.EndTime;
            ret.Name = contest.Name;
            ret.Password = contest.Password;
            ret.Config = contest.Config.DeserializeJson<ContestConfig>(false);

            return ret;
        }

        [HttpPost]
        [RequireTeacher]
        [Route("edit")]
        public async Task UpdateContest([FromBody]ContestEditModel model)
        {
            var contest = await contestService.GetContestAsync(model.Id);
            if (contest == null) throw new NotFoundException("找不到该比赛");

            contest.Description = model.Description;
            contest.Hidden = model.Hidden;
            contest.StartTime = model.StartTime;
            contest.EndTime = model.EndTime;
            contest.Name = model.Name;
            contest.Password = model.Password;
            contest.Config = model.Config.SerializeJsonAsString(false);

            await contestService.UpdateContestAsync(contest);
            await contestService.UpdateContestProblemAsync(contest.Id, model.Problems);
        }

        [HttpGet]
        [RequireTeacher]
        [Route("edit")]
        public async Task<ContestEditModel> GetContest(int contestId)
        {
            var userId = userManager.GetUserId(User);
            var ret = new ContestEditModel();

            var contest = await contestService.GetContestAsync(contestId);
            if (contest == null) throw new NotFoundException("找不到该比赛");

            ret.Description = contest.Description;
            ret.Hidden = contest.Hidden;
            ret.Id = contest.Id;
            ret.StartTime = contest.StartTime;
            ret.EndTime = contest.EndTime;
            ret.Name = contest.Name;
            ret.Password = contest.Password;
            ret.Config = contest.Config.DeserializeJson<ContestConfig>(false);

            var problems = await problemService.QueryProblemAsync(userId, contestId);
            ret.Problems = problems.Select(i => i.Id).ToList();
            return ret;
        }
    }
}
