using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    public partial class AdminController : Controller
    {
        public class ContestIdModel : ResultModel
        {
            public int Id { get; set; }
        }

        [HttpGet]
        public async Task<ContestEditModel> GetContestConfig(int cid)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var model = new ContestEditModel { IsSucceeded = true };
            if (!HasTeacherPrivilege(privilege))
            {
                model.IsSucceeded = false;
                model.ErrorMessage = "没有权限";
                return model;
            }
            if (cid == 0)
            {
                var current = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00"));
                model.StartTime = current;
                model.EndTime = current.AddHours(5);
                return model;
            }

            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contest = await db.Contest.FindAsync(cid);
                if (contest == null)
                {
                    model.IsSucceeded = false;
                    model.ErrorMessage = "比赛不存在";
                    return model;
                }
                model.Id = contest.Id;
                model.Name = contest.Name;
                model.Config = JsonConvert.DeserializeObject<ContestConfiguration>(contest.Config ?? "{}");
                model.Description = contest.Description;
                model.Hidden = contest.Hidden;
                model.StartTime = contest.StartTime;
                model.EndTime = contest.EndTime;
                model.Password = contest.Password;
                model.SpecifyCompetitors = contest.SpecifyCompetitors;
                foreach (var item in db.ContestRegister.Where(i => i.ContestId == cid).Select(i => i.UserId))
                {
                    var competitor = await _userManager.FindByIdAsync(item);
                    if (competitor == null)
                    {
                        continue;
                    }

                    model.Competitors.Add(new Competitor
                    {
                        Id = competitor.Id,
                        Email = competitor.Email,
                        Name = competitor.Name,
                        UserName = competitor.UserName
                    });
                }
                model.ProblemSet = db.ContestProblemConfig.Where(i => i.ContestId == cid).Select(i => i.ProblemId).ToList().Aggregate(string.Empty, (accu, next) => accu + next.ToString() + "; ");
                return model;
            }
        }

        [HttpPost]
        public async Task<ResultModel> DeleteContest([FromBody]ContestIdModel model)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var ret = new ResultModel { IsSucceeded = true };
            if (!HasTeacherPrivilege(privilege))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "没有权限";
                return ret;
            }

            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contest = await db.Contest.FindAsync(model.Id);
                if (contest == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "比赛不存在";
                    return ret;
                }
                db.Contest.Remove(contest);

                await db.SaveChangesAsync();

                return ret;
            }
        }

        [HttpPost]
        public async Task<ContestIdModel> UpdateContestConfig([FromBody]ContestEditModel model)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var ret = new ContestIdModel { IsSucceeded = true };
            if (!HasTeacherPrivilege(privilege))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "没有权限";
                return ret;
            }


            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var contest = model.Id == 0 ? new Contest() : await db.Contest.FindAsync(model.Id);
                if (contest == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "比赛不存在";
                    return ret;
                }

                contest.Name = model.Name;
                contest.Config = JsonConvert.SerializeObject(model.Config);
                contest.Description = model.Description;
                contest.Hidden = model.Hidden;
                contest.StartTime = model.StartTime;
                contest.EndTime = model.EndTime;
                contest.SpecifyCompetitors = model.SpecifyCompetitors;
                contest.Password = model.Password;

                if (model.Id == 0)
                {
                    contest.UserId = user.Id;
                    await db.Contest.AddAsync(contest);
                };
                await db.SaveChangesAsync();
                db.ContestRegister.RemoveRange(db.ContestRegister.Where(i => i.ContestId == contest.Id));
                if (contest.SpecifyCompetitors)
                {
                    foreach (var competitor in model.Competitors)
                    {
                        db.ContestRegister.Add(new ContestRegister { ContestId = contest.Id, UserId = competitor.Id });
                    }
                }
                if (!string.IsNullOrEmpty(model.ProblemSet))
                {
                    var problemSet = model.ProblemSet.Trim().Split(';', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(i => int.Parse(i.Trim()));
                    foreach (var i in db.ContestProblemConfig.Where(i => i.ContestId == contest.Id))
                    {
                        if (!problemSet.Contains(i.ProblemId))
                        {
                            db.ContestProblemConfig.Remove(i);
                        }
                    }
                    foreach (var pid in problemSet)
                    {
                        if (db.ContestProblemConfig.Any(i => i.ProblemId == pid && i.ContestId == contest.Id))
                        {
                            continue;
                        }

                        if (db.Problem.Any(i => i.Id == pid))
                        {
                            db.ContestProblemConfig.Add(new ContestProblemConfig { ProblemId = pid, ContestId = contest.Id, AcceptCount = 0, SubmissionCount = 0 });
                        }
                    }
                }
                else
                {
                    db.ContestProblemConfig.RemoveRange(db.ContestProblemConfig.Where(i => i.ContestId == contest.Id));
                }
                await db.SaveChangesAsync();
                ret.Id = contest.Id;
                return ret;
            }
        }
    }
}