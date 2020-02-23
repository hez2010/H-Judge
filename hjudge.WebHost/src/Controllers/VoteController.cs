using System.Threading.Tasks;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models;
using hjudge.WebHost.Models.Vote;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [ApiController]
    [Route("vote")]
    public class VoteController : ControllerBase
    {
        private readonly IVoteService voteService;
        private readonly UserManager<UserInfo> userManager;

        public VoteController(IVoteService voteService, UserManager<UserInfo> userManager)
        {
            this.voteService = voteService;
            this.userManager = userManager;
        }

        /// <summary>
        /// 为题目投票
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [PrivilegeAuthentication.RequireSignedIn]
        [Route("problem")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ErrorModel))]
        public async Task VoteProblem([FromBody]ProblemVoteModel model)
        {
            var userId = userManager.GetUserId(User);
            var result = model.VoteType switch
            {
                1 => await voteService.UpvoteProblemAsync(userId, model.ProblemId),
                2 => await voteService.DownvoteProblemAsync(userId, model.ProblemId),
                _ => false
            };
            if (!result) throw new BadRequestException("评价失败");
        }

        /// <summary>
        /// 为比赛投票
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [PrivilegeAuthentication.RequireSignedIn]
        [Route("contest")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ErrorModel))]
        public async Task VoteContest([FromBody]ContestVoteModel model)
        {
            var userId = userManager.GetUserId(User);
            var result = model.VoteType switch
            {
                1 => await voteService.UpvoteContestAsync(userId, model.ContestId),
                2 => await voteService.DownvoteContestAsync(userId, model.ContestId),
                _ => false
            };
            if (!result) throw new BadRequestException("评价失败");
        }

        /// <summary>
        /// 取消投票
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [PrivilegeAuthentication.RequireSignedIn]
        [Route("cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ErrorModel))]
        public async Task CancelVote([FromBody]CancelVoteModel model)
        {
            var userId = userManager.GetUserId(User);
            var result = model switch
            {
                { ProblemId: null, ContestId: null } => false,
                { ProblemId: null, ContestId: _ } => await voteService.CancelVoteContestAsync(userId, model.ContestId.Value),
                { ProblemId: _, ContestId: null } => await voteService.CancelVoteProblemAsync(userId, model.ProblemId.Value),
                _ => false
            };
            if (!result) throw new BadRequestException("评价取消失败");
        }
    }
}