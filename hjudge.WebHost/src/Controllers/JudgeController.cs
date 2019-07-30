using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;

namespace hjudge.WebHost.Controllers
{
    [Route("api/[controller]")]
    [AutoValidateAntiforgeryToken]
    [ApiController]
    public class JudgeController : ControllerBase
    {
        private readonly IJudgeService judgeService;
        private readonly CachedUserManager<UserInfo> userManager;

        public JudgeController(IJudgeService judgeService, CachedUserManager<UserInfo> userManager)
        {
            this.judgeService = judgeService;
            this.userManager = userManager;
        }

        public class SubmitModel
        {
            public int ProblemId;
            public int ContestId;
            public int GroupId;
            public string Language = string.Empty;
            public string Content = string.Empty;
        }

        [PrivilegeAuthentication.RequireSignedIn]
        [HttpPost]
        [Route("Submit")]
        public async Task<ResultModel> Submit([FromBody]SubmitModel model)
        {
            var ret = new ResultModel();
            var userId = userManager.GetUserId(User);
            //TODO: validate

            await judgeService.QueueJudgeAsync(new Judge
            {
                Content = model.Content,
                Language = model.Language,
                ProblemId = model.ProblemId,
                ContestId = model.ContestId == 0 ? null : (int?)model.ContestId,
                GroupId = model.GroupId == 0 ? null : (int?)model.GroupId,
                UserId = userId
            });
            return ret;
        }
    }
}
