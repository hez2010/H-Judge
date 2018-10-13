using hjudgeCore;
using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models.Contest;
using hjudgeWeb.Models.Problem;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    public class ContestController : Controller
    {
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly UserManager<UserInfo> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public ContestController(SignInManager<UserInfo> signInManager,
            UserManager<UserInfo> userManager,
            DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContextOptions = dbContextOptions;
        }

        private async Task<(UserInfo, int)> GetUserPrivilegeAsync()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return (null, 0);
            }
            var user = await _userManager.GetUserAsync(User);
            return (user, user?.Privilege ?? 0);
        }

        private bool HasAdminPrivilege(int privilege)
        {
            return privilege >= 1 && privilege <= 3;
        }

        [HttpGet]
        public async Task<int> GetContestCount()
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contests = HasAdminPrivilege(privilege) ? db.Contest : db.Contest.Where(i => !i.Hidden);
                return await contests.CountAsync();
            }
        }

        [HttpGet]
        public async Task<List<ContestListItemModel>> GetContestList(int start = 0, int count = 10)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contests = HasAdminPrivilege(privilege) ? db.Contest.OrderByDescending(i => i.Id) : db.Contest.Where(i => !i.Hidden).OrderByDescending(i => i.Id);
                var list = contests.Skip(start).Take(count).Select(i => new ContestListItemModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Hidden = i.Hidden,
                    RawStartTime = i.StartTime,
                    RawEndTime = i.EndTime,
                    ProblemCount = db.ContestProblemConfig.Count(j => j.ContestId == i.Id)
                }).ToList();
                foreach (var item in list)
                {
                    item.Status = "进行中";
                    if (item.RawEndTime < DateTime.Now)
                    {
                        item.Status = "已结束";
                    }
                    if (item.RawStartTime > DateTime.Now)
                    {
                        item.Status = "未开始";
                    }
                }
                return list;
            }
        }

        [HttpGet]
        public async Task<ContestDetailsModel> GetContestDetails(int cid)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();

            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contest = await db.Contest.FindAsync(cid);
                var ret = new ContestDetailsModel { IsSucceeded = true };
                if (contest == null || (contest.Hidden && !HasAdminPrivilege(privilege)))
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "找不到该比赛";
                }

                ret.Id = contest.Id;
                ret.Name = contest.Name;
                ret.UserId = contest.UserId;
                ret.UserName = (await _userManager.FindByIdAsync(contest.UserId))?.UserName;
                ret.RawStartTime = contest.StartTime;
                ret.RawEndTime = contest.EndTime;
                ret.Password = contest.Password;
                ret.Status = "进行中";
                if (ret.RawEndTime < DateTime.Now)
                {
                    ret.Status = "已结束";
                }
                if (ret.RawStartTime > DateTime.Now)
                {
                    ret.Status = "未开始";
                }
                ret.Description = contest.Description;
                ret.ProblemCount = db.ContestProblemConfig.Count(j => j.ContestId == contest.Id);
                return ret;
            }
        }

        [HttpGet]
        public async Task<int> GetProblemCount(int cid)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contest = await db.Contest.FindAsync(cid);
                if (contest == null || (contest.Hidden && !HasAdminPrivilege(privilege)))
                {
                    return 0;
                }

                return db.ContestProblemConfig.Count(i => i.ContestId == cid && db.Problem.Any(j => j.Id == i.ProblemId));
            }
        }

        [HttpGet]
        public async Task<List<ProblemListItemModel>> GetProblemList(int cid, int start = 0, int count = 10)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contest = await db.Contest.FindAsync(cid);
                var list = new List<ProblemListItemModel>();
                if (contest == null || (contest.Hidden && !HasAdminPrivilege(privilege)))
                {
                    return list;
                }

                if (contest.StartTime > DateTime.Now)
                {
                    return list;
                }

                int? contestId = null;
                if (cid != 0)
                {
                    contestId = cid;
                }

                foreach (var i in db.ContestProblemConfig.Where(i => i.ContestId == cid).Select(i => new { i.ProblemId, i.AcceptCount, i.SubmissionCount }))
                {
                    var problem = await db.Problem.FindAsync(i.ProblemId);
                    if (problem == null)
                    {
                        continue;
                    }
                    var item = new ProblemListItemModel
                    {
                        Id = problem.Id,
                        Name = problem.Name,
                        RawCreationTime = problem.CreationTime,
                        RawLevel = problem.Level,
                        RawType = problem.Type,
                        AcceptCount = i.AcceptCount ?? 0,
                        SubmissionCount = i.SubmissionCount ?? 0
                    };

                    if (user != null)
                    {
                        var submissions = db.Judge.Where(j => j.ContestId == contestId && j.ProblemId == problem.Id && j.UserId == user.Id);
                        if (submissions.Any())
                        {
                            item.RawStatus = 1;
                        }
                        if (submissions.Any(j => j.ResultType == (int)ResultCode.Accepted))
                        {
                            item.RawStatus = 2;
                        }
                    }
                    list.Add(item);
                }
                return list;
            }
        }

        [HttpGet]
        public async Task<ContestRankModel> GetRank(int cid, int gid = 0)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var ret = new ContestRankModel { IsSucceeded = true };
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contest = await db.Contest.FindAsync(cid);
                if (contest == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "比赛不存在";
                    return ret;
                }
                if (gid != 0)
                {
                    if (!db.GroupContestConfig.Any(i => i.ContestId == cid && i.GroupId == gid))
                    {
                        ret.IsSucceeded = false;
                        ret.ErrorMessage = "比赛不存在";
                        return ret;
                    }

                    if (!db.GroupJoin.Any(i => i.GroupId == gid && i.UserId == user.Id))
                    {
                        if (!HasAdminPrivilege(privilege))
                        {
                            ret.IsSucceeded = false;
                            ret.ErrorMessage = "没有权限";
                            return ret;
                        }
                    }
                }
                var config = JsonConvert.DeserializeObject<ContestConfiguration>(contest.Config ?? "{}");
                if (!config.ShowRank)
                {
                    if (!HasAdminPrivilege(privilege))
                    {
                        ret.IsSucceeded = false;
                        ret.ErrorMessage = "没有权限";
                        return ret;
                    }
                }

                var problems = db.ContestProblemConfig.Where(i => i.ContestId == cid);
                foreach (var item in problems)
                {
                    var pid = item.ProblemId ?? 0;
                    var problemName = db.Problem.Select(i => new { i.Id, i.Name }).FirstOrDefault(i => i.Id == pid)?.Name;
                    ret.ProblemInfo[pid] = new RankProblemInfo
                    {
                        Id = pid,
                        Name = problemName,
                        AcceptedCount = item.AcceptCount ?? 0,
                        SubmissionCount = item.SubmissionCount ?? 0
                    };
                }

                var judges = gid == 0 ? db.Judge.Where(i => i.ContestId == cid && i.GroupId == null) : db.Judge.Where(i => i.ContestId == cid && i.GroupId == gid);
                judges = judges.OrderBy(i => i.Id);
                if (config.AutoStopRank)
                {
                    judges = judges.Where(i => i.JudgeTime.AddHours(1) < contest.EndTime);
                }

                foreach (var i in judges.GroupBy(i => i.UserId))
                {
                    if (!ret.RankInfo.ContainsKey(i.Key))
                    {
                        var competitor = await _userManager.FindByIdAsync(i.Key);
                        if (competitor == null)
                        {
                            continue;
                        }

                        ret.RankInfo[i.Key] = new SingleUserRankInfo
                        {
                            UserInfo = new RankUserInfo
                            {
                                Id = i.Key,
                                UserName = competitor.UserName,
                                Name = HasAdminPrivilege(privilege) ? competitor.Name : null
                            }
                        };
                    }

                    foreach (var judgeProblem in i.GroupBy(j => j.ProblemId))
                    {
                        foreach (var j in judgeProblem)
                        {
                            int pid = j.ProblemId ?? 0;
                            if (pid == 0)
                            {
                                continue;
                            }

                            if (!ret.RankInfo[i.Key].SubmitInfo.ContainsKey(pid))
                            {
                                ret.RankInfo[i.Key].SubmitInfo[pid] = new RankSubmitInfo();
                            }
                            if (j.ResultType != (int)ResultCode.Pending && j.ResultType != (int)ResultCode.Judging)
                            {
                                ret.RankInfo[i.Key].SubmitInfo[pid].SubmissionCount++;
                                ret.RankInfo[i.Key].SubmitInfo[pid].TimeCost += j.JudgeTime - contest.StartTime;

                                if (config.Type == ContestType.LastSubmit)
                                {
                                    ret.RankInfo[i.Key].SubmitInfo[pid].IsAccepted = false;
                                }
                                if (j.ResultType == (int)ResultCode.Accepted)
                                {
                                    ret.RankInfo[i.Key].SubmitInfo[pid].IsAccepted = true;
                                    ret.RankInfo[i.Key].SubmitInfo[pid].Score = j.FullScore;
                                    if (config.Type != ContestType.LastSubmit)
                                    {
                                        break;
                                    }
                                }
                                else if (j.ResultType != (int)ResultCode.Compile_Error
                                    && j.ResultType != (int)ResultCode.Special_Judge_Error
                                    && j.ResultType != (int)ResultCode.Problem_Config_Error
                                    && j.ResultType != (int)ResultCode.Unknown_Error)
                                {
                                    if (config.Type == ContestType.Penalty)
                                    {
                                        ret.RankInfo[i.Key].SubmitInfo[pid].PenaltyCount++;
                                    }
                                }

                                if (config.ScoreMode == ScoreCountingMode.All)
                                {
                                    if (config.Type == ContestType.LastSubmit)
                                    {
                                        ret.RankInfo[i.Key].SubmitInfo[pid].Score = j.FullScore;
                                    }
                                    else
                                    {
                                        ret.RankInfo[i.Key].SubmitInfo[pid].Score = Math.Max(ret.RankInfo[i.Key].SubmitInfo[pid].Score, j.FullScore);
                                    }
                                }
                            }
                        }
                    }
                }

                var ranked = ret.RankInfo.OrderByDescending(i => i.Value.FullScore).ThenBy(i => i.Value.TimeCost).ToList();
                for (var i = 0; i < ranked.Count; i++)
                {
                    ranked[i].Value.Rank = i + 1;
                }
                ret.RankInfo = ranked.ToDictionary(i => i.Key, i => i.Value);
                return ret;
            }
        }
    }
}