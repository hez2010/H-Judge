using System;
using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hjudge.Core;
using hjudge.WebHost.Models.Problem;
using hjudge.WebHost.Services;
using hjudge.WebHost.Configurations;
using hjudge.Shared.Utils;
using EFSecondLevelCache.Core;
using hjudge.WebHost.Utils;
using hjudge.WebHost.Models;
using static hjudge.WebHost.Middlewares.PrivilegeAuthentication;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;
using hjudge.WebHost.Exceptions;
using System.Net;
using Google.Protobuf;
using System.Collections.Generic;
using System.Text;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("problem")]
    [ApiController]
    public class ProblemController : ControllerBase
    {
        private readonly CachedUserManager<UserInfo> userManager;
        private readonly IProblemService problemService;
        private readonly IJudgeService judgeService;
        private readonly ILanguageService languageService;
        private readonly IFileService fileService;
        private readonly WebHostDbContext dbContext;

        public ProblemController(
            CachedUserManager<UserInfo> userManager,
            IProblemService problemService,
            IJudgeService judgeService,
            ILanguageService languageService,
            IFileService fileService,
            WebHostDbContext dbContext)
        {
            this.userManager = userManager;
            this.problemService = problemService;
            this.judgeService = judgeService;
            this.languageService = languageService;
            this.fileService = fileService;
            this.dbContext = dbContext;
        }

        private readonly static int[] allStatus = new[] { 0, 1, 2 };

        [HttpPost]
        [Route("list")]
        public async Task<ProblemListModel> ProblemList([FromBody]ProblemListQueryModel model)
        {
            var userId = userManager.GetUserId(User);

            var ret = new ProblemListModel();

            var judges = await judgeService.QueryJudgesAsync(userId)
                (model.GroupId == 0 ? null : (int?)model.GroupId)
                (model.ContestId == 0 ? null : (int?)model.ContestId)
                (0);

            IQueryable<Problem> problems;

            try
            {
                problems = await (model switch
                {
                    { ContestId: 0, GroupId: 0 } => problemService.QueryProblemAsync(userId),
                    { GroupId: 0 } => problemService.QueryProblemAsync(userId, model.ContestId),
                    { } => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId)
                });
            }
            catch (Exception ex)
            {
                throw new InterfaceException((HttpStatusCode)ex.HResult, ex.Message);
            }

            if (model.Filter.Id != 0)
            {
                problems = problems.Where(i => i.Id == model.Filter.Id);
            }
            if (!string.IsNullOrEmpty(model.Filter.Name))
            {
                problems = problems.Where(i => i.Name.Contains(model.Filter.Name));
            }

            if (model.Filter.Status.Count < 3)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    foreach (var status in allStatus)
                    {
                        if (!model.Filter.Status.Contains(status))
                        {
                            problems = status switch
                            {
                                0 => problems.Where(i => judges.Any(j => j.ProblemId == i.Id)),
                                1 => problems.Where(i => !judges.Any(j => j.ProblemId == i.Id && j.ResultType != (int)ResultCode.Accepted)),
                                2 => problems.Where(i => !judges.Any(j => j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted)),
                                _ => problems
                            };
                        }
                    }
                }
            }

            if (model.RequireTotalCount) ret.TotalCount = await problems.Select(i => i.Id).Cacheable().CountAsync();

            if (model.ContestId == 0) problems = problems.OrderBy(i => i.Id);
            else model.StartId = 0; // keep original order while fetching problems in a contest

            if (model.StartId == 0) problems = problems.Skip(model.Start);
            else problems = problems.Where(i => i.Id >= model.StartId);

            if (model.ContestId != 0)
            {
                ret.Problems = await problems.Include(i => i.ContestProblemConfig).Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Level = i.Level,
                    AcceptCount = i.ContestProblemConfig.FirstOrDefault(j => j.ContestId == model.ContestId && j.ProblemId == i.Id).AcceptCount,
                    SubmissionCount = i.ContestProblemConfig.FirstOrDefault(j => j.ContestId == model.ContestId && j.ProblemId == i.Id).SubmissionCount,
                    Hidden = false,
                    Upvote = i.Upvote,
                    Downvote = i.Downvote,
                    Status = judges.Any(j => j.ProblemId == i.Id) ?
                        (judges.Any(j => j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted) ? 2 : 1) : 0
                }).Cacheable().ToListAsync();
            }
            else
            {
                ret.Problems = await problems.Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Level = i.Level,
                    AcceptCount = i.AcceptCount,
                    SubmissionCount = i.SubmissionCount,
                    Hidden = i.Hidden,
                    Upvote = i.Upvote,
                    Downvote = i.Downvote,
                    Status = judges.Any(j => j.ProblemId == i.Id) ?
                        (judges.Any(j => j.ProblemId == i.Id && j.ResultType == (int)ResultCode.Accepted) ? 2 : 1) : 0
                }).Cacheable().ToListAsync();
            }

            return ret;
        }

        [HttpPost]
        [Route("details")]
        public async Task<ProblemModel> ProblemDetails([FromBody]ProblemQueryModel model)
        {
            var userId = userManager.GetUserId(User);
            var ret = new ProblemModel();

            IQueryable<Problem> problems;
            try
            {
                problems = await (model switch
                {
                    { ContestId: 0, GroupId: 0 } => problemService.QueryProblemAsync(userId),
                    { GroupId: 0 } => problemService.QueryProblemAsync(userId, model.ContestId),
                    { } => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId)
                });
            }
            catch (Exception ex)
            {
                throw new InterfaceException((HttpStatusCode)ex.HResult, ex.Message);
            }

            var problem = await problems.Where(i => i.Id == model.ProblemId).Cacheable().FirstOrDefaultAsync();
            if (problem == null) throw new NotFoundException("找不到该题目");

            var judges = await judgeService.QueryJudgesAsync(userId)
                (model.GroupId == 0 ? null : (int?)model.GroupId)
                (model.ContestId == 0 ? null : (int?)model.ContestId)
                (0);

            if (await judges.Where(i => i.ProblemId == problem.Id)
                    .Cacheable().AnyAsync())
            {
                ret.Status = 1;
                if (await judges.Where(i => i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted)
                        .Cacheable().AnyAsync())
                {
                    ret.Status = 2;
                }
            }

            ret.AcceptCount = problem.AcceptCount;
            ret.SubmissionCount = problem.SubmissionCount;

            if (model.ContestId != 0)
            {
                var data = await dbContext.ContestProblemConfig
                    .Where(i => i.ContestId == model.ContestId && i.ProblemId == problem.Id)
                    .Select(i => new { i.AcceptCount, i.SubmissionCount })
                    .Cacheable()
                    .FirstOrDefaultAsync();
                if (data != null)
                {
                    ret.AcceptCount = data.AcceptCount;
                    ret.SubmissionCount = data.SubmissionCount;
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                if (await judges.Where(i => i.ProblemId == problem.Id).Cacheable().AnyAsync())
                {
                    ret.Status = 1;
                    if (await judges.Where(i => i.ProblemId == problem.Id && i.ResultType == (int)ResultCode.Accepted)
                            .Cacheable().AnyAsync())
                    {
                        ret.Status = 2;
                    }
                }
            }

            var user = await userManager.FindByIdAsync(problem.UserId);
            ret.Name = problem.Name;
            ret.Hidden = problem.Hidden;
            ret.Level = problem.Level;
            ret.Type = problem.Type;
            ret.UserId = problem.UserId;
            ret.UserName = user?.UserName ?? string.Empty;
            ret.Id = problem.Id;
            ret.Description = problem.Description;
            ret.CreationTime = problem.CreationTime;
            ret.Upvote = problem.Upvote;
            ret.Downvote = problem.Downvote;

            var config = problem.Config.DeserializeJson<ProblemConfig>(false);

            var langConfig = await languageService.GetLanguageConfigAsync();
            var langs = config?.Languages?.Split(';', StringSplitOptions.RemoveEmptyEntries);

            ret.Languages = LanguageConfigHelper.GenerateLanguageConfig(langConfig, langs).ToList();

            return ret;
        }

        [HttpDelete]
        [RequireAdmin]
        [Route("edit")]
        public Task RemoveProblem(int problemId)
        {
            return problemService.RemoveProblemAsync(problemId);
        }

        [HttpPut]
        [RequireAdmin]
        [Route("edit")]
        public async Task<ProblemEditModel> CreateProblem([FromBody]ProblemEditModel model)
        {
            var userId = userManager.GetUserId(User);

            var problem = new Problem
            {
                Description = model.Description,
                Hidden = model.Hidden,
                Level = model.Level,
                Name = model.Name,
                Type = model.Type,
                CreationTime = DateTime.Now,
                UserId = userId,
                Config = model.Config.SerializeJsonAsString(false)
            };

            problem.Id = await problemService.CreateProblemAsync(problem);
            Directory.CreateDirectory($"AppData/Data/{problem.Id}");

            return new ProblemEditModel
            {
                Description = problem.Description,
                Hidden = problem.Hidden,
                Id = problem.Id,
                Level = problem.Level,
                Name = problem.Name,
                Type = problem.Type,
                Config = problem.Config.DeserializeJson<ProblemConfig>(false)
            };
        }

        [HttpPost]
        [RequireAdmin]
        [Route("edit")]
        public async Task UpdateProblem([FromBody]ProblemEditModel model)
        {
            var problem = await problemService.GetProblemAsync(model.Id);
            if (problem == null) throw new NotFoundException("找不到该题目");

            problem.Description = model.Description;
            problem.Hidden = model.Hidden;
            problem.Level = model.Level;
            problem.Name = model.Name;
            problem.Type = model.Type;
            problem.Config = model.Config.SerializeJsonAsString(false);

            await problemService.UpdateProblemAsync(problem);
        }

        [HttpGet]
        [RequireAdmin]
        [Route("edit")]
        public async Task<ProblemEditModel> GetProblem(int problemId)
        {
            var problem = await problemService.GetProblemAsync(problemId);
            if (problem == null) throw new NotFoundException("找不到该题目");

            return new ProblemEditModel
            {
                Description = problem.Description,
                Hidden = problem.Hidden,
                Id = problem.Id,
                Level = problem.Level,
                Name = problem.Name,
                Type = problem.Type,
                Config = problem.Config.DeserializeJson<ProblemConfig>(false)
            };
        }

        [HttpPut]
        [RequireAdmin]
        [Route("data")]
        [RequestSizeLimit(135000000)]
        public async Task<ProblemDataUploadModel> UploadData([FromForm]int problemId, IFormFile file)
        {
            if ((await problemService.GetProblemAsync(problemId)) == null) throw new NotFoundException("找不到该题目");
            if (file.ContentType != "application/x-zip-compressed" && file.ContentType != "application/zip") throw new BadRequestException("文件格式不正确");
            if (file.Length > 134217728) throw new BadRequestException("文件大小不能超过 128 Mb");

            using var stream = file.OpenReadStream();
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read, false, Encoding.UTF8);
            var list = new List<UploadInfo>();
            var failedList = new ProblemDataUploadModel();
            long size = 0;
            foreach (var i in zip.Entries.Where(i => !i.FullName.EndsWith("/")))
            {
                var entryStream = i.Open();
                size += i.Length;
                if (size > 140 * 1048576)
                {
                    await foreach (var j in fileService.UploadFilesAsync(list)) if (!j.Succeeded) failedList.FailedFiles.Add(j.FileName);
                    list.Clear();
                    list.Add(new UploadInfo { FileName = $"Data/{problemId}/{i.FullName}", Content = await ByteString.FromStreamAsync(entryStream) });
                    size = i.Length;
                }
                else list.Add(new UploadInfo { FileName = $"Data/{problemId}/{i.FullName}", Content = await ByteString.FromStreamAsync(entryStream) });
            }
            await foreach (var j in fileService.UploadFilesAsync(list)) if (!j.Succeeded) failedList.FailedFiles.Add(j.FileName);
            return failedList;
        }

        [HttpGet]
        [RequireAdmin]
        [Route("data")]
        public async Task<IActionResult> GetData(int problemId)
        {
            if ((await problemService.GetProblemAsync(problemId)) == null) throw new NotFoundException("找不到该题目");

            var files = await fileService.ListFilesAsync($"Data/{problemId}/");
            var downloadedFiles = fileService.DownloadFilesAsync(files);
            var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                await foreach (var i in downloadedFiles)
                {
                    var entry = zip.CreateEntry(i.FileName);
                    using var entryStream = entry.Open();
                    i.Content.WriteTo(entryStream);
                }
            }
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/x-zip-compressed", $"Data_{problemId}_{DateTime.Now:yyyyMMddHHmmssffff}.zip", true);
        }

        [HttpDelete]
        [RequireAdmin]
        [Route("data")]
        public async Task DeleteData(int problemId)
        {
            if ((await problemService.GetProblemAsync(problemId)) == null) return;

            var files = await fileService.ListFilesAsync($"Data/{problemId}/");
            _ = await fileService.DeleteFilesAsync(files);
        }
    }
}