using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hjudge.Core;
using hjudge.Shared.Utils;
using hjudge.WebHost.Configurations;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models;
using hjudge.WebHost.Models.Judge;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;

namespace hjudge.WebHost.Controllers
{
    [Route("judge")]
    [AutoValidateAntiforgeryToken]
    [ApiController]
    public class JudgeController : ControllerBase
    {
        private readonly IJudgeService judgeService;
        private readonly IContestService contestService;
        private readonly CachedUserManager<UserInfo> userManager;

        public JudgeController(IJudgeService judgeService, IContestService contestService, CachedUserManager<UserInfo> userManager)
        {
            this.judgeService = judgeService;
            this.contestService = contestService;
            this.userManager = userManager;
        }


        [PrivilegeAuthentication.RequireSignedIn]
        [HttpPost]
        [Route("submit")]
        public async Task SubmitSolution([FromBody]SubmitModel model)
        {
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
        }

        [Route("result")]
        [HttpGet]
        [PrivilegeAuthentication.RequireSignedIn]
        public async Task<ResultModel> GetJudgeResult(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var judge = await judgeService.GetJudgeAsync(id);
            if (judge == null) throw new NotFoundException("评测结果不存在");
            if (!Utils.PrivilegeHelper.IsTeacher(user.Privilege) && judge.UserId != user.Id && !judge.IsPublic) throw new ForbiddenException("没有权限查看该评测结果");

            var ret = new ResultModel
            {
                ResultId = judge.Id,
                UserId = judge.UserId,
                UserName = judge.UserInfo.UserName,
                ProblemId = judge.ProblemId,
                ProblemName = judge.Problem.Name,
                ContestId = judge.ContestId,
                ContestName = judge.Contest?.Name,
                GroupId = judge.GroupId,
                GroupName = judge.Group?.Name,
                ResultType = judge.ResultType,
                Content = judge.Content,
                Time = judge.JudgeTime
            };
            ret.JudgeResult = (string.IsNullOrWhiteSpace(judge.Result) ? "{}" : judge.Result).DeserializeJson<JudgeResult>(false);
            ret.JudgeResult.JudgePoints ??= new List<JudgePoint>();

            if (judge.ContestId != null && !Utils.PrivilegeHelper.IsTeacher(user.Privilege))
            {
                var contest = await contestService.GetContestAsync(judge.ContestId.Value);
                if (contest != null)
                {
                    var config = contest.Config.DeserializeJson<ContestConfig>(false);
                    if (!config.CanMakeResultPublic && !Utils.PrivilegeHelper.IsTeacher(user.Privilege) && judge.UserId != user.Id) throw new ForbiddenException("没有权限查看该评测结果");
                    switch (config.ResultMode)
                    {
                        case ResultDisplayMode.Never:
                            throw new ForbiddenException("当前不允许查看该评测结果");
                        case ResultDisplayMode.AfterContest:
                            if (DateTime.Now < contest.EndTime) throw new ForbiddenException("当前不允许查看该评测结果");
                            break;
                    }
                    ret.ResultType = ResultDisplayType.Summary switch
                    {
                        ResultDisplayType.Detailed => 0,
                        ResultDisplayType.Summary => 1,
                        _ => 0
                    };
                }
            }

            return ret;
        }
    }
}
