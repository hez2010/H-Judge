using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hjudge.Core;
using hjudge.Shared.Utils;
using hjudge.WebHost.Configurations;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Middlewares;
using hjudge.WebHost.Models.Judge;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EFSecondLevelCache.Core;

namespace hjudge.WebHost.Controllers
{
    [Route("judge")]
    [AutoValidateAntiforgeryToken]
    [ApiController]
    public class JudgeController : ControllerBase
    {
        private readonly IJudgeService judgeService;
        private readonly IProblemService problemService;
        private readonly IContestService contestService;
        private readonly IGroupService groupService;
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly ILanguageService languageService;

        public JudgeController(IJudgeService judgeService, IProblemService problemService,
            IContestService contestService, IGroupService groupService, CachedUserManager<UserInfo> userManager,
            ILanguageService languageService)
        {
            this.judgeService = judgeService;
            this.problemService = problemService;
            this.contestService = contestService;
            this.groupService = groupService;
            this.userManager = userManager;
            this.languageService = languageService;
        }

        [PrivilegeAuthentication.RequireSignedIn]
        [HttpPost]
        [Route("rejudge")]
        public async Task Rejudge([FromBody]RejudgeModel model)
        {
            var judge = await judgeService.GetJudgeAsync(model.ResultId);
            if (judge == null) throw new NotFoundException("该提交不存在");

            await judgeService.QueueJudgeAsync(judge);
        }

        [PrivilegeAuthentication.RequireSignedIn]
        [HttpPost]
        [RequestSizeLimit(10485760)]
        [Route("submit")]
        public async Task<SubmitSuccessModel> SubmitSolution([FromBody]SubmitModel model)
        {
            var user = await userManager.GetUserAsync(User);
            var now = DateTime.Now;
            var allowJumpToResult = true;

            if (user.Privilege == 5) throw new ForbiddenException("不允许提交，请与管理员联系");

            if (model.GroupId != 0)
            {
                var inGroup = await groupService.IsInGroupAsync(user.Id, model.GroupId);
                if (!inGroup) throw new ForbiddenException("未参加该小组");
            }

            var problem = await problemService.GetProblemAsync(model.ProblemId);
            if (problem == null) throw new NotFoundException("该题目不存在");
            var problemConfig = problem.Config.DeserializeJson<ProblemConfig>(false);

            if (problemConfig.CodeSizeLimit != 0 && problemConfig.CodeSizeLimit < Encoding.UTF8.GetByteCount(model.Content))
                throw new BadRequestException("提交内容长度超出限制");

            var langConfig = (await languageService.GetLanguageConfigAsync()).ToList();
            var langs = problemConfig.Languages?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            if (langs.Length == 0) langs = langConfig.Select(i => i.Name).ToArray();

            if (model.ContestId != 0)
            {
                var contest = await contestService.GetContestAsync(model.ContestId);
                if (contest != null)
                {
                    if (contest.StartTime > now || now > contest.EndTime) throw new ForbiddenException("当前不允许提交");
                    if (contest.Hidden && !Utils.PrivilegeHelper.IsTeacher(user.Privilege)) throw new NotFoundException("该比赛不存在");

                    var contestConfig = contest.Config.DeserializeJson<ContestConfig>(false);
                    if (contestConfig.SubmissionLimit != 0)
                    {
                        var judges = await judgeService.QueryJudgesAsync(user.Id,
                                model.GroupId == 0 ? null : (int?)model.GroupId,
                                model.ContestId,
                                model.ProblemId);
                        if (contestConfig.SubmissionLimit <= await judges/*.Cacheable()*/.CountAsync())
                            throw new ForbiddenException("超出提交次数限制");
                    }
                    if (contestConfig.ResultMode != ResultDisplayMode.Intime) allowJumpToResult = false;
                    var contestLangs = contestConfig.Languages?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
                    if (contestLangs.Length != 0) langs = langs.Intersect(contestLangs).ToArray();
                }
            }
            else if (problem.Hidden && !Utils.PrivilegeHelper.IsTeacher(user.Privilege)) throw new NotFoundException("该题目不存在");

            if (!langs.Contains(model.Language)) throw new ForbiddenException("不允许使用该语言提交");

            user.SubmissionCount++;
            await userManager.UpdateAsync(user);

            var id = await judgeService.QueueJudgeAsync(new Judge
            {
                Content = model.Content,
                Language = model.Language,
                ProblemId = model.ProblemId,
                ContestId = model.ContestId == 0 ? null : (int?)model.ContestId,
                GroupId = model.GroupId == 0 ? null : (int?)model.GroupId,
                UserId = user.Id,
                Description = "Online Judge"
            });

            return new SubmitSuccessModel
            {
                Jump = allowJumpToResult,
                ResultId = id
            };
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
                Time = judge.JudgeTime,
                Language = judge.Language,
                JudgeResult = (string.IsNullOrWhiteSpace(judge.Result) ? "{}" : judge.Result)
                    .DeserializeJson<JudgeResult>(false)
            };
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
