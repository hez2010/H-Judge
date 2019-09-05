using hjudge.Shared.Utils;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Models.Rank;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using Microsoft.EntityFrameworkCore;
using hjudge.WebHost.Configurations;
using hjudge.Core;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [ApiController]
    [Route("rank")]
    public class RankController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IJudgeService judgeService;
        private readonly IContestService contestService;
        private readonly IGroupService groupService;

        public RankController(CachedUserManager<UserInfo> userManager, IJudgeService judgeService, IContestService contestService, IGroupService groupService)
        {
            this.userManager = userManager;
            this.judgeService = judgeService;
            this.contestService = contestService;
            this.groupService = groupService;
        }

        [Route("contest")]
        [HttpGet]
        public async Task<RankContestStatisticsModel> GetRankForContest(int contestId, int groupId)
        {
            var user = await userManager.GetUserAsync(User);
            var contest = await contestService.GetContestAsync(contestId);
            if (contest == null) throw new NotFoundException("该比赛不存在");
            if (groupId != 0)
            {
                var groups = await groupService.QueryGroupAsync(user?.Id);
                groups = groups.Where(i => i.GroupContestConfig.Any(j => j.ContestId == contestId && j.GroupId == groupId))/*.Cacheable()*/;
                if (!await groups.AnyAsync()) throw new NotFoundException("该比赛不存在或未加入对应小组");
            }

            var config = contest.Config.DeserializeJson<ContestConfig>();
            if (!config.ShowRank && !Utils.PrivilegeHelper.IsTeacher(user?.Privilege)) throw new ForbiddenException("不允许查看排名");

            var judges = await judgeService.QueryJudgesAsync(null,
                groupId == 0 ? null : (int?)groupId,
                contestId,
                0);

            if (config.AutoStopRank && !Utils.PrivilegeHelper.IsTeacher(user?.Privilege) && DateTime.Now < contest.EndTime)
            {
                var time = contest.EndTime.AddHours(-1);
                judges = judges.Where(i => i.JudgeTime < time);
            }

            var ret = new RankContestStatisticsModel
            {
                ContestId = contestId,
                GroupId = groupId
            };

            var results = judges.Include(i => i.UserInfo).Include(i => i.Problem).OrderBy(i => i.Id).Select(i => new
            {
                Id = i.Id,
                ProblemId = i.ProblemId,
                ProblemName = i.Problem.Name,
                UserId = i.UserId,
                UserName = i.UserInfo.UserName,
                Name = i.UserInfo.Name,
                ResultType = i.ResultType,
                Time = i.JudgeTime,
                Score = i.FullScore
            })/*.Cacheable()*/;

            var isAccepted = new Dictionary<(string UserId, int ProblemId), bool>();

            foreach (var i in results)
            {
                if (!ret.UserInfos.ContainsKey(i.UserId)) ret.UserInfos[i.UserId] = new RankUserInfoModel
                {
                    UserName = i.UserName,
                    Name = Utils.PrivilegeHelper.IsTeacher(user?.Privilege) ? i.Name : string.Empty
                };
                if (!ret.ProblemInfos.ContainsKey(i.ProblemId)) ret.ProblemInfos[i.ProblemId] = new RankProblemInfoModel
                {
                    ProblemName = i.ProblemName
                };
                if (!ret.RankInfos.ContainsKey(i.UserId)) ret.RankInfos[i.UserId] = new Dictionary<int, RankContestItemModel>();
                if (!ret.RankInfos[i.UserId].ContainsKey(i.ProblemId)) ret.RankInfos[i.UserId][i.ProblemId] = new RankContestItemModel();

                if (config.Type != ContestType.LastSubmit)
                    ret.RankInfos[i.UserId][i.ProblemId].Accepted = ret.RankInfos[i.UserId][i.ProblemId].Accepted || (i.ResultType == (int)ResultCode.Accepted);
                else
                    ret.RankInfos[i.UserId][i.ProblemId].Accepted = ret.RankInfos[i.UserId][i.ProblemId].Accepted && (i.ResultType == (int)ResultCode.Accepted);

                if (!isAccepted.ContainsKey((i.UserId, i.ProblemId)))
                    isAccepted[(i.UserId, i.ProblemId)] = false;

                var (penalty, time) = (isAccepted[(i.UserId, i.ProblemId)], config.Type == ContestType.Penalty, i.ResultType) switch
                {
                    (true, _, _) => (0, TimeSpan.Zero), // if has accepted, there will be no more time accumulation and penalty
                    (_, true, (int)ResultCode.Compile_Error) => (0, TimeSpan.Zero),
                    (_, true, _) => (20, i.Time - contest.StartTime + TimeSpan.FromMinutes(20)),
                    (_, false, (int)ResultCode.Compile_Error) => (0, TimeSpan.Zero),
                    _ => (0, i.Time - contest.StartTime)
                };

                var score = (config.Type, config.ScoreMode) switch
                {
                    (ContestType.LastSubmit, ScoreCountingMode.All) => i.Score,
                    (ContestType.LastSubmit, _) => i.ResultType == (int)ResultCode.Accepted ? i.Score : 0,
                    (_, ScoreCountingMode.All) => Math.Max(i.Score, ret.RankInfos[i.UserId][i.ProblemId].Score),
                    _ => Math.Max(i.ResultType == (int)ResultCode.Accepted ? i.Score : 0, ret.RankInfos[i.UserId][i.ProblemId].Score),
                };

                ret.RankInfos[i.UserId][i.ProblemId].Penalty += penalty;
                ret.RankInfos[i.UserId][i.ProblemId].Time += time;
                ret.RankInfos[i.UserId][i.ProblemId].Score = score;

                if (i.ResultType == (int)ResultCode.Accepted)
                {
                    isAccepted[(i.UserId, i.ProblemId)] = true;
                    ret.RankInfos[i.UserId][i.ProblemId].AcceptCount++;
                    ret.ProblemInfos[i.ProblemId].AcceptCount++;
                }

                ret.RankInfos[i.UserId][i.ProblemId].SubmissionCount++;
                ret.ProblemInfos[i.ProblemId].SubmissionCount++;

                ret.UserInfos[i.UserId].Penalty += penalty;
                ret.UserInfos[i.UserId].Time += time;
            }

            foreach (var i in ret.UserInfos)
            {
                i.Value.Score = ret.RankInfos[i.Key].Sum(j => j.Value.Score);
            }

            // no time and penalty if hasn't accepted
            foreach (var i in isAccepted)
            {
                if (!i.Value)
                {
                    ret.UserInfos[i.Key.UserId].Time -= ret.RankInfos[i.Key.UserId][i.Key.ProblemId].Time;
                    ret.UserInfos[i.Key.UserId].Penalty -= ret.RankInfos[i.Key.UserId][i.Key.ProblemId].Penalty;
                    ret.RankInfos[i.Key.UserId][i.Key.ProblemId].Time = TimeSpan.Zero;
                    ret.RankInfos[i.Key.UserId][i.Key.ProblemId].Penalty = 0;
                }
            }

            var rankData = ret.UserInfos.Select(i => new { UserId = i.Key, Score = i.Value.Score, Time = i.Value.Time })
                .OrderByDescending(i => i.Score)
                .ThenBy(i => i.Time)
                .ToList();

            var sameRankCnt = 0;

            for (var i = 0; i < rankData.Count; i++)
            {
                ret.UserInfos[rankData[i].UserId].Rank = i + 1 - sameRankCnt;
                if (i != 0)
                {
                    if (Math.Abs(rankData[i].Score - rankData[i - 1].Score) < 0.001 && rankData[i].Time == rankData[i - 1].Time) sameRankCnt++;
                    else sameRankCnt = 0;
                }
            }

            return ret;
        }
    }
}
