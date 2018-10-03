using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    public partial class AdminController : Controller
    {
        [HttpGet]
        public async Task<ProblemEditModel> GetProblemConfig(int pid)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var model = new ProblemEditModel { IsSucceeded = true };
            if (!HasAdminPrivilege(privilege))
            {
                model.IsSucceeded = false;
                model.ErrorMessage = "没有权限";
                return model;
            }
            if (pid == 0)
            {
                return model;
            }

            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problem = await db.Problem.FindAsync(pid);
                if (problem == null)
                {
                    model.IsSucceeded = false;
                    model.ErrorMessage = "题目不存在";
                    return model;
                }
                model.Id = problem.Id;
                model.Name = problem.Name;
                model.Level = problem.Level;
                model.Config = JsonConvert.DeserializeObject<ProblemConfiguration>(problem.Config ?? "{}");
                model.Description = problem.Description;
                model.Type = problem.Type;
                model.Hidden = problem.Hidden;
                return model;
            }
        }

        [HttpPost]
        public async Task<ResultModel> UpdateProblemConfig([FromBody]ProblemEditModel model)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var ret = new ProblemEditModel { IsSucceeded = true };
            if (!HasAdminPrivilege(privilege))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "没有权限";
                return ret;
            }


            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problem = model.Id == 0 ? new Problem() : await db.Problem.FindAsync(model.Id);
                if (problem == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "题目不存在";
                    return ret;
                }

                problem.Name = model.Name;
                problem.Level = model.Level;
                problem.Config = JsonConvert.SerializeObject(model.Config);
                problem.Type = model.Type;
                problem.Description = model.Description;
                problem.Hidden = model.Hidden;

                if (model.Id == 0)
                {
                    problem.CreationTime = DateTime.Now;
                    problem.UserId = user.Id;
                    await db.Problem.AddAsync(problem);
                };

                await db.SaveChangesAsync();

                return ret;
            }
        }
    }
}