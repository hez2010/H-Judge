using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models.Contest;
using hjudgeWeb.Models.Problem;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<int> GetContestCount()
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contests = HasAdminPrivilege(privilege) ? db.Contest : db.Contest.Where(i => !i.Hidden);
                return await contests.CountAsync();
            }
        }

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

                foreach (var i in db.ContestProblemConfig.Where(i => i.ContestId == cid).Select(i => new { i.ProblemId, i.AcceptCount, i.SubmissionCount }))
                {
                    var problem = await db.Problem.FindAsync(i.ProblemId);
                    if (problem == null)
                    {
                        continue;
                    }

                    list.Add(new ProblemListItemModel
                    {
                        Id = problem.Id,
                        Name = problem.Name,
                        RawCreationTime = problem.CreationTime,
                        RawLevel = problem.Level,
                        RawType = problem.Type,
                        AcceptCount = i.AcceptCount ?? 0,
                        SubmissionCount = i.SubmissionCount ?? 0
                    });
                }
                return list;
            }
        }
    }
}