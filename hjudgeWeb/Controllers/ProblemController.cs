using hjudgeCore;
using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    public class ProblemController : Controller
    {
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly UserManager<UserInfo> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public ProblemController(SignInManager<UserInfo> signInManager,
            UserManager<UserInfo> userManager,
            DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContextOptions = dbContextOptions;
        }

        private async Task<int> GetUserPrivilegeAsync()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return 0;
            }
            var user = await _userManager.GetUserAsync(User);
            return user?.Privilege ?? 0;
        }

        private int GetUserPrivilegeAsync(UserInfo user)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return 0;
            }
            return user?.Privilege ?? 0;
        }

        /// <summary>
        /// 获取题目列表
        /// </summary>
        /// <param name="start">起始 index</param>
        /// <param name="count">数量</param>
        /// <returns>题目列表</returns>
        [HttpGet]
        public async Task<List<ProblemListViewModel>> GetProblemList(int start = 0, int count = 20)
        {
            var privilege = await GetUserPrivilegeAsync();
            var user = await _userManager.GetUserAsync(User);
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problems = privilege >= 1 && privilege <= 4 ? db.Problem : db.Problem.Where(i => !i.Hidden);
                var list = problems.Skip(start).Take(count).Select(i => new ProblemListViewModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    AcceptCount = i.AcceptCount ?? 0,
                    SubmissionCount = i.SubmissionCount ?? 0,
                    CreationTime = i.CreationTime,
                    Level = i.Level
                }).ToList();
                foreach (var i in list)
                {
                    var submissions = db.Judge.Where(j => j.GroupId == null && j.ContestId == null);
                    if (submissions.Any())
                    {
                        i.Status = 1;
                    }
                    if (submissions.Any(j => j.ResultType == (int)ResultCode.Accepted))
                    {
                        i.Status = 2;
                    }
                }
                return list;
            }
        }

        [HttpGet]
        public async Task<ProblemViewModel> GetProblemDetails(int pid)
        {
            throw new NotImplementedException();
        }
    }
}
