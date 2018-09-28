using hjudgeCore;
using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Models.Problem;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        
        /// <summary>
        /// 获取题目列表
        /// </summary>
        /// <param name="quantity">数量信息</param>
        /// <returns>题目列表</returns>
        [HttpGet]
        public async Task<List<ProblemListItemModel>> GetProblemList(int start = 0, int count = 20)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problems = HasAdminPrivilege(privilege) ? db.Problem : db.Problem.Where(i => !i.Hidden);
                var list = problems.Skip(start).Take(count).Select(i => new ProblemListItemModel
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
        public async Task<ProblemDetailsModel> GetProblemDetails(int pid)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problem = await db.Problem.FindAsync(pid);
                if (problem == null)
                {
                    return null;
                }
                if (problem.Hidden)
                {
                    if (!HasAdminPrivilege(privilege))
                    {
                        return new ProblemDetailsModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "没有权限"
                        };
                    }
                }
                var author = await _userManager.FindByIdAsync(problem.UserId);
                return new ProblemDetailsModel
                {
                    IsSucceeded = true,
                    Id = problem.Id,
                    Hidden = problem.Hidden,
                    Level = problem.Level,
                    Name = problem.Name,
                    Type = problem.Type,
                    UserId = problem.UserId,
                    UserName = $"{author?.UserName}",
                    Description = problem.Description,
                    CreationTime = problem.CreationTime,
                    AcceptCount = problem.AcceptCount ?? 0,
                    SubmissionCount = problem.SubmissionCount ?? 0
                };
            }
        }
    }
}
