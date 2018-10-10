using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO.Compression;
using System.Threading.Tasks;

namespace hjudgeWeb.Controllers
{
    [Consumes("application/json", "multipart/form-data")]
    public partial class AdminController : Controller
    {
        public class ProblemIdModel : ResultModel
        {
            public int Id { get; set; }
        }

        [HttpGet]
        public async Task<ProblemEditModel> GetProblemConfig(int pid)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var model = new ProblemEditModel { IsSucceeded = true };
            if (!HasTeacherPrivilege(privilege))
            {
                model.IsSucceeded = false;
                model.ErrorMessage = "没有权限";
                return model;
            }
            if (pid == 0)
            {
                model.Level = 1;
                model.Type = 1;
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
        public async Task<ResultModel> DeleteProblem([FromBody]ProblemIdModel model)
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
                var problem = await db.Problem.FindAsync(model.Id);
                if (problem == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "题目不存在";
                    return ret;
                }
                db.Problem.Remove(problem);

                await db.SaveChangesAsync();

                var datadir = System.IO.Path.Combine(Environment.CurrentDirectory, "AppData", "Data", model.Id.ToString());
                if (System.IO.Directory.Exists(datadir))
                {
                    System.IO.Directory.Delete(datadir, true);
                }

                return ret;
            }
        }

        [HttpPost]
        public async Task<ProblemIdModel> UpdateProblemConfig([FromBody]ProblemEditModel model)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            var ret = new ProblemIdModel { IsSucceeded = true };
            if (!HasTeacherPrivilege(privilege))
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
                ret.Id = problem.Id;
                return ret;
            }
        }

        [HttpPost]
        public async Task<ResultModel> DeleteProblemData([FromBody]ProblemIdModel model)
        {
            var ret = new ResultModel { IsSucceeded = true };
            var (user, privilege) = await GetUserPrivilegeAsync();
            if (!HasTeacherPrivilege(privilege))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "没有权限";
                return ret;
            }

            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problem = await db.Problem.FindAsync(model.Id);
                if (problem == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "找不到此题目";
                    return ret;
                }
            }

            var datadir = System.IO.Path.Combine(Environment.CurrentDirectory, "AppData", "Data", model.Id.ToString());
            if (System.IO.Directory.Exists(datadir))
            {
                System.IO.Directory.Delete(datadir, true);
            }
            return ret;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadProblemData(int id)
        {
            var (user, privilege) = await GetUserPrivilegeAsync();
            if (!HasTeacherPrivilege(privilege))
            {
                throw new InvalidOperationException("没有权限");
            }

            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problem = await db.Problem.FindAsync(id);
                if (problem == null)
                {
                    throw new InvalidOperationException("找不到此题目");
                }
            }
            var datadir = System.IO.Path.Combine(Environment.CurrentDirectory, "AppData", "Data", id.ToString());
            var downloaddir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Download");
            if (!System.IO.Directory.Exists(downloaddir))
            {
                System.IO.Directory.CreateDirectory(downloaddir);
            }

            if (!System.IO.Directory.Exists(datadir))
            {
                System.IO.Directory.CreateDirectory(datadir);
            }
            var fileName = System.IO.Path.Combine(downloaddir, Guid.NewGuid() + ".zip");
            ZipFile.CreateFromDirectory(datadir, fileName);
            
            return File(new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite),
                "application/x-zip-compressed", $"ProblemData_{id}.zip");
        }

        [HttpPost]
        [RequestSizeLimit(135000000)]
        public async Task<ResultModel> UploadProblemData([FromForm]int pid, IFormFile file)
        {
            var ret = new ResultModel { IsSucceeded = true };
            var (user, privilege) = await GetUserPrivilegeAsync();
            if (!HasTeacherPrivilege(privilege))
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "没有权限";
                return ret;
            }

            if (file == null)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "文件无效";
                return ret;
            }

            if (file.ContentType != "application/x-zip-compressed" && file.ContentType != "application/zip")
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "文件格式不正确";
                return ret;
            }

            if (file.Length > 134217728)
            {
                ret.IsSucceeded = false;
                ret.ErrorMessage = "文件大小超出限制";
                return ret;
            }

            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var problem = await db.Problem.FindAsync(pid);
                if (problem == null)
                {
                    ret.IsSucceeded = false;
                    ret.ErrorMessage = "找不到此题目";
                    return ret;
                }
            }
            if (!System.IO.Directory.Exists(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Upload")))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Upload"));
            }
            var fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Upload", Guid.NewGuid() + ".zip");
            using (var stream = new System.IO.FileStream(fileName, System.IO.FileMode.CreateNew))
            {
                file.CopyTo(stream);
            }

            var datadir = System.IO.Path.Combine(Environment.CurrentDirectory, "AppData", "Data", pid.ToString());
            if (!System.IO.Directory.Exists(datadir))
            {
                System.IO.Directory.CreateDirectory(datadir);
            }

            ZipFile.ExtractToDirectory(fileName, datadir, true);

            try
            {
                System.IO.File.Delete(fileName);
            }
            catch
            { /* ignored */ }

            return ret;
        }
    }
}