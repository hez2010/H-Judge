using hjudgeCore;
using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
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

        [HttpGet]
        public async Task<int> GetProblemCount()
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problems = HasAdminPrivilege(privilege) ? db.Problem : db.Problem.Where(i => !i.Hidden);
                return await problems.CountAsync();
            }
        }

        /// <summary>
        /// 获取题目列表
        /// </summary>
        /// <param name="quantity">数量信息</param>
        /// <returns>题目列表</returns>
        [HttpGet]
        public async Task<List<ProblemListItemModel>> GetProblemList(int start = 0, int count = 10, int cid = 0, int gid = 0)
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
                    RawType = i.Type,
                    SubmissionCount = i.SubmissionCount ?? 0,
                    RawCreationTime = i.CreationTime,
                    RawLevel = i.Level
                }).ToList();

                foreach (var i in list)
                {
                    int? groupId = null, contestId = null;
                    if (gid != 0)
                    {
                        groupId = gid;
                    }
                    if (cid != 0)
                    {
                        contestId = cid;
                    }

                    var submissions = db.Judge.Where(j => j.GroupId == groupId && j.ContestId == contestId && j.ProblemId == i.Id);
                    i.RawStatus = 0;
                    if (submissions.Any())
                    {
                        i.RawStatus = 1;
                    }
                    if (submissions.Any(j => j.ResultType == (int)ResultCode.Accepted))
                    {
                        i.RawStatus = 2;
                    }
                }
                return list;
            }
        }

        public class SubmitModel
        {
            public int Pid { get; set; }
            public int Cid { get; set; }
            public int Gid { get; set; }
            public string Content { get; set; }
            public string Language { get; set; }
        }

        [HttpPost]
        public async Task<SubmitReturnDataModel> Submit([FromBody]SubmitModel submit)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                if (submit.Gid != 0)
                {
                    var group = await db.Group.FindAsync(submit.Gid);
                    if (group == null)
                    {
                        return new SubmitReturnDataModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }

                    if (!db.GroupJoin.Any(i => i.GroupId == submit.Gid && i.UserId == user.Id))
                    {
                        if (!HasAdminPrivilege(privilege))
                        {
                            return new SubmitReturnDataModel
                            {
                                IsSucceeded = false,
                                ErrorMessage = "没有权限"
                            };
                        }
                    }

                    if (!db.GroupContestConfig.Where(i => i.GroupId == submit.Gid).Any(i => i.ContestId == submit.Cid))
                    {
                        return new SubmitReturnDataModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }
                }

                if (submit.Cid != 0)
                {
                    var contest = await db.Contest.FindAsync(submit.Cid);
                    if (contest == null)
                    {
                        return new SubmitReturnDataModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }

                    if (contest.Hidden)
                    {
                        if (!HasAdminPrivilege(privilege))
                        {
                            return new SubmitReturnDataModel
                            {
                                IsSucceeded = false,
                                ErrorMessage = "没有权限"
                            };
                        }
                    }

                    if (!db.ContestProblemConfig.Where(i => i.ContestId == submit.Cid).Any(i => i.ProblemId == submit.Pid))
                    {
                        return new SubmitReturnDataModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }
                }

                var problem = await db.Problem.FindAsync(submit.Pid);
                if (problem == null)
                {
                    return new SubmitReturnDataModel
                    {
                        IsSucceeded = false,
                        ErrorMessage = "题目不存在"
                    };
                }
                if (problem.Hidden)
                {
                    if (!HasAdminPrivilege(privilege))
                    {
                        return new SubmitReturnDataModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "没有权限"
                        };
                    }
                }
                var submission = new Data.Judge
                {
                    ProblemId = submit.Pid,
                    Content = submit.Content,
                    UserId = user.Id,
                    JudgeTime = DateTime.Now,
                    Description = "Online Judge",
                    Language = submit.Language
                };
                if (submit.Cid != 0)
                {
                    submission.ContestId = submit.Cid;
                }
                if (submit.Gid != 0)
                {
                    submission.GroupId = submit.Gid;
                }

                db.Judge.Add(submission);
                await db.SaveChangesAsync();

                return new SubmitReturnDataModel
                {
                    Id = submission.Id,
                    IsSucceeded = true,
                    Redirect = true
                };
            }
        }

        [HttpGet]
        public async Task<ProblemDetailsModel> GetProblemDetails(int pid, int cid = 0, int gid = 0)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                if (gid != 0)
                {
                    var group = await db.Group.FindAsync(gid);
                    if (group == null)
                    {
                        return new ProblemDetailsModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }

                    if (!db.GroupJoin.Any(i => i.GroupId == gid && i.UserId == user.Id))
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

                    if (!db.GroupContestConfig.Where(i => i.GroupId == gid).Any(i => i.ContestId == cid))
                    {
                        return new ProblemDetailsModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }
                }

                if (cid != 0)
                {
                    var contest = await db.Contest.FindAsync(cid);
                    if (contest == null)
                    {
                        return new ProblemDetailsModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }

                    if (contest.Hidden)
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

                    if (!db.ContestProblemConfig.Where(i => i.ContestId == cid).Any(i => i.ProblemId == pid))
                    {
                        return new ProblemDetailsModel
                        {
                            IsSucceeded = false,
                            ErrorMessage = "题目不存在"
                        };
                    }
                }

                var problem = await db.Problem.FindAsync(pid);
                if (problem == null)
                {
                    return new ProblemDetailsModel
                    {
                        IsSucceeded = false,
                        ErrorMessage = "题目不存在"
                    };
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

                var problemDetails = new ProblemDetailsModel
                {
                    IsSucceeded = true,
                    Id = problem.Id,
                    Hidden = problem.Hidden,
                    RawLevel = problem.Level,
                    Name = problem.Name,
                    RawType = problem.Type,
                    UserId = problem.UserId,
                    UserName = $"{author?.UserName}",
                    Description = problem.Description,
                    RawCreationTime = problem.CreationTime,
                    AcceptCount = problem.AcceptCount ?? 0,
                    SubmissionCount = problem.SubmissionCount ?? 0,
                    Languages = Languages.LanguagesList
                };
                int? groupId = null, contestId = null;
                if (gid != 0)
                {
                    groupId = gid;
                }
                if (cid != 0)
                {
                    contestId = cid;
                }

                var submissions = db.Judge.Where(j => j.GroupId == groupId && j.ContestId == contestId && j.ProblemId == pid);
                problemDetails.RawStatus = 0;
                if (submissions.Any())
                {
                    problemDetails.RawStatus = 1;
                }
                if (submissions.Any(j => j.ResultType == (int)ResultCode.Accepted))
                {
                    problemDetails.RawStatus = 2;
                }

                return problemDetails;
            }
        }
    }
}
