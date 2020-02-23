using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using hjudge.Shared.Utils;
using hjudge.WebHost.Configurations;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models;
using hjudge.WebHost.Models.Account;
using hjudge.WebHost.Models.Contest;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static hjudge.WebHost.Middlewares.PrivilegeAuthentication;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("contest")]
    [ApiController]
    public class ContestController : ControllerBase
    {
        private readonly UserManager<UserInfo> userManager;
        private readonly IContestService contestService;
        private readonly IProblemService problemService;
        private readonly IVoteService voteService;

        public ContestController(
            UserManager<UserInfo> userManager,
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

        /// <summary>
        /// 查询比赛
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("list")]
        [ProducesResponseType(200)]
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

            if (model.RequireTotalCount) ret.TotalCount = await contests.Select(i => i.Id).CountAsync();

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
            }).ToListAsync();

            ret.CurrentTime = DateTime.Now;
            return ret;
        }

        /// <summary>
        /// 获取比赛详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("details")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
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

            var contest = await contests.Where(i => i.Id == model.ContestId).FirstOrDefaultAsync();

            if (contest is null) throw new NotFoundException("找不到该比赛");

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

        /// <summary>
        /// 删除比赛
        /// </summary>
        /// <param name="contestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
        public Task RemoveContest(int contestId)
        {
            return contestService.RemoveContestAsync(contestId);
        }

        /// <summary>
        /// 创建比赛
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
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

        /// <summary>
        /// 更新比赛
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
        public async Task UpdateContest([FromBody]ContestEditModel model)
        {
            var contest = await contestService.GetContestAsync(model.Id);
            if (contest is null) throw new NotFoundException("找不到该比赛");

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

        /// <summary>
        /// 获取比赛和比赛配置
        /// </summary>
        /// <param name="contestId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
        public async Task<ContestEditModel> GetContest(int contestId)
        {
            var userId = userManager.GetUserId(User);
            var ret = new ContestEditModel();

            var contest = await contestService.GetContestAsync(contestId);
            if (contest is null) throw new NotFoundException("找不到该比赛");

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

        /// <summary>
        /// 获取参赛者，该 API 当前未启用
        /// </summary>
        /// <param name="contestId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequireTeacher]
        [FunctionalControl.DisabledApi]
        [Route("competitors")]
        [ProducesResponseType(200)]
        public async Task<List<UserBasicInfoModel>> GetCompetitorsListAsync(int contestId)
        {
            return await (await contestService.QueryCompetitorsAsync(contestId))
                .Select(i => new UserBasicInfoModel
                {
                    UserId = i.Id,
                    Name = i.Name,
                    UserName = i.UserName,
                    Email = i.Email
                }).ToListAsync();
        }

        /// <summary>
        /// 将用户加入比赛，该 API 当前未启用
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequireTeacher]
        [FunctionalControl.DisabledApi]
        [Route("competitors/add")]
        [ProducesResponseType(200)]
        public Task JoinContestAsync([FromBody]JoinQuitContestModel model)
        {
            return contestService.JoinContestAsync(model.ContestId, model.UserIds);
        }

        /// <summary>
        /// 将用户移出比赛，该 API 当前未启用
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequireTeacher]
        [FunctionalControl.DisabledApi]
        [Route("competitors/remove")]
        [ProducesResponseType(200)]
        public Task QuitContestAsync([FromBody]JoinQuitContestModel model)
        {
            return contestService.QuitContestAsync(model.ContestId, model.UserIds);
        }
    }
}
