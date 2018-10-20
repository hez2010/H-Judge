using hjudgeCore;
using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Status;
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
    [AutoValidateAntiforgeryToken]
    public class StatusController : Controller
    {
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly UserManager<UserInfo> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public StatusController(SignInManager<UserInfo> signInManager,
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
        public async Task<int> GetStatusCount(int pid = 0, int cid = 0, int gid = 0, bool onlyMe = false)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                IQueryable<Judge> list;
                if (cid == 0 && gid == 0)
                {
                    list = db.Judge.OrderByDescending(i => i.Id).Where(i => i.ContestId == null && i.GroupId == null);
                }
                else
                {
                    var contest = await db.Contest.FindAsync(cid);
                    if (contest != null)
                    {
                        var config = JsonConvert.DeserializeObject<ContestConfiguration>(contest.Config ?? "{}");
                        if (config != null)
                        {
                            if (config.ResultMode == ResultDisplayMode.Never || (config.ResultMode == ResultDisplayMode.AfterContest && contest.EndTime > DateTime.Now))
                            {
                                return 0;
                            }
                        }
                    }

                    if (gid != 0)
                    {
                        list = db.Judge.OrderByDescending(i => i.Id).Where(i => i.ContestId == cid && i.GroupId == gid);
                    }
                    else
                    {
                        list = db.Judge.OrderByDescending(i => i.Id).Where(i => i.ContestId == cid && i.GroupId == null);
                    }
                }

                if (pid != 0)
                {
                    list = list.Where(i => i.ProblemId == pid);
                }

                if (user != null && onlyMe)
                {
                    list = list.Where(i => i.UserId == user.Id);
                }

                return await list.CountAsync();
            }
        }

        [HttpGet]
        public async Task<List<StatusListItemModel>> GetStatusList(int start = 0, int count = 10, int pid = 0, int cid = 0, int gid = 0, bool onlyMe = false)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                IQueryable<Judge> list;
                string contestName = null, groupName = null;
                ContestConfiguration config = null;

                if (cid == 0 && gid == 0)
                {
                    list = db.Judge.OrderByDescending(i => i.Id).Where(i => i.ContestId == null && i.GroupId == null);
                }
                else
                {
                    var contest = await db.Contest.Select(i => new { i.Id, i.Config, i.EndTime, i.Name }).FirstOrDefaultAsync(i => i.Id == cid);

                    if (contest != null)
                    {
                        contestName = contest.Name;

                        config = JsonConvert.DeserializeObject<ContestConfiguration>(contest.Config ?? "{}");
                        if (!HasAdminPrivilege(privilege))
                        {
                            if (config != null)
                            {
                                if (config.ResultMode == ResultDisplayMode.Never || (config.ResultMode == ResultDisplayMode.AfterContest && contest.EndTime > DateTime.Now))
                                {
                                    return new List<StatusListItemModel>();
                                }
                            }
                        }
                    }

                    if (gid != 0)
                    {
                        var group = await db.Group.Select(i => new { i.Id, i.Name }).FirstOrDefaultAsync(i => i.Id == gid);

                        if (group != null)
                        {
                            groupName = group.Name;
                        }

                        list = db.Judge.OrderByDescending(i => i.Id).Where(i => i.ContestId == cid && i.GroupId == gid);
                    }
                    else
                    {
                        list = db.Judge.OrderByDescending(i => i.Id).Where(i => i.ContestId == cid && i.GroupId == null);
                    }
                }

                if (pid != 0)
                {
                    list = list.Where(i => i.ProblemId == pid);
                }

                if (user != null && onlyMe)
                {
                    list = list.Where(i => i.UserId == user.Id);
                }

                var result = await list.Skip(start).Take(count).Select(i => new StatusListItemModel
                {
                    Id = i.Id,
                    GroupId = i.GroupId ?? 0,
                    ContestId = i.ContestId ?? 0,
                    FullScore = i.FullScore,
                    Language = i.Language,
                    ProblemId = i.ProblemId ?? 0,
                    RawJudgeTime = i.JudgeTime,
                    ResultType = i.ResultType,
                    UserId = i.UserId,
                    RawType = i.Type
                }).ToListAsync();

                foreach (var i in result)
                {
                    if (i.ContestId != 0)
                    {
                        i.ContestName = contestName;
                    }
                    if (i.GroupId != 0)
                    {
                        i.GroupName = groupName;
                    }
                    if (i.ProblemId != 0)
                    {
                        i.ProblemName = db.Problem.Select(j => new { j.Id, j.Name }).FirstOrDefault(j => j.Id == i.ProblemId)?.Name;
                    }
                    if (i.UserId != null)
                    {
                        i.UserName = (await _userManager.FindByIdAsync(i.UserId))?.UserName;
                    }
                    if (config != null && config.ScoreMode == ScoreCountingMode.OnlyAccepted)
                    {
                        if (i.ResultType != (int)ResultCode.Accepted)
                        {
                            i.FullScore = 0;
                        }
                    }
                }
                return result;
            }
        }

        public class SubmitModel
        {
            public int Jid { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> Rejudge([FromBody]SubmitModel submit)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var ret = new ResultModel { IsSucceeded = true };
            if (!HasAdminPrivilege(privilege))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "没有权限";
            }
            else
            {
                using (var db = new ApplicationDbContext(_dbContextOptions))
                {
                    var judge = await db.Judge.FindAsync(submit.Jid);
                    if (judge == null)
                    {
                        ret.IsSucceeded = false;
                        ret.ErrorMessage = "找不到该提交";
                    }
                    else
                    {
                        if (judge.ResultType <= 0)
                        {
                            ret.IsSucceeded = false;
                            ret.ErrorMessage = "请等待上一次评测完成";
                        }
                    }
                    if (ret.IsSucceeded)
                    {
                        judge.ResultType = -1;
                        await db.SaveChangesAsync();
                        JudgeQueue.JudgeIdQueue.Enqueue(judge.Id);
                    }
                }
            }
            return ret;
        }

        [HttpGet]
        public async Task<JudgeResultModel> GetJudgeResult(int jid)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var ret = new JudgeResultModel { IsSucceeded = true };
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var judge = await db.Judge.FindAsync(jid);
                if (judge == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "找不到该结果";
                    return ret;
                }
                if (!HasAdminPrivilege(privilege) && judge.UserId != user?.Id)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "没有权限";
                    return ret;
                }

                ret.Id = judge.Id;
                ret.ContestId = judge.ContestId ?? 0;
                ret.GroupId = judge.GroupId ?? 0;
                ret.ProblemId = judge.ProblemId ?? 0;
                ret.UserId = judge.UserId;
                ret.RawJudgeTime = judge.JudgeTime;
                ret.ResultType = judge.ResultType;
                ret.JudgeResult = JsonConvert.DeserializeObject<JudgeResult>(judge.Result ?? "{}");
                ret.Language = judge.Language;
                ret.FullScore = judge.FullScore;
                ret.Content = judge.Content;
                ret.RawType = judge.Type;

                var contest = await db.Contest.Select(i => new { i.Id, i.Config, i.EndTime, i.Name }).FirstOrDefaultAsync(i => i.Id == (judge.ContestId ?? 0));

                if (contest != null)
                {
                    var config = JsonConvert.DeserializeObject<ContestConfiguration>(contest.Config ?? "{}");
                    if (config != null)
                    {
                        if (!HasAdminPrivilege(privilege))
                        {
                            if (config.ResultMode == ResultDisplayMode.Never || (config.ResultMode == ResultDisplayMode.AfterContest && contest.EndTime > DateTime.Now))
                            {
                                ret.IsSucceeded = false;
                                ret.ErrorMessage = "不允许查看";
                                return ret;
                            }
                            if (config.ResultType == ResultDisplayType.Summary)
                            {
                                ret.JudgeResult.JudgePoints = new List<JudgePoint>();
                                ret.JudgeResult.CompileLog = string.Empty;
                                ret.JudgeResult.StaticCheckLog = string.Empty;
                            }
                        }
                        if (config.ScoreMode == ScoreCountingMode.OnlyAccepted && ret.ResultType != (int)ResultCode.Accepted)
                        {
                            ret.FullScore = 0;
                        }
                    }
                }

                if (ret.ContestId != 0)
                {
                    ret.ContestName = contest?.Name;
                }
                if (ret.GroupId != 0)
                {
                    ret.GroupName = db.Group.Select(j => new { j.Id, j.Name }).FirstOrDefault(j => j.Id == ret.GroupId)?.Name;
                }
                if (ret.ProblemId != 0)
                {
                    ret.ProblemName = db.Problem.Select(j => new { j.Id, j.Name }).FirstOrDefault(j => j.Id == ret.ProblemId)?.Name;
                }
                if (ret.UserId != null)
                {
                    ret.UserName = (await _userManager.FindByIdAsync(ret.UserId))?.UserName;
                }

                return ret;
            }
        }
    }
}