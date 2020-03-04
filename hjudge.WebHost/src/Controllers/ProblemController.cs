using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor;
using Google.Protobuf;
using hjudge.Core;
using hjudge.Shared.Utils;
using hjudge.WebHost.Configurations;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Models;
using hjudge.WebHost.Models.Problem;
using hjudge.WebHost.Services;
using hjudge.WebHost.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static hjudge.WebHost.Middlewares.PrivilegeAuthentication;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("problem")]
    [ApiController]
    public class ProblemController : ControllerBase
    {
        private readonly UserManager<UserInfo> userManager;
        private readonly IProblemService problemService;
        private readonly IContestService contestService;
        private readonly IJudgeService judgeService;
        private readonly ILanguageService languageService;
        private readonly IFileService fileService;
        private readonly IVoteService voteService;
        private readonly WebHostDbContext dbContext;

        public ProblemController(
            UserManager<UserInfo> userManager,
            IProblemService problemService,
            IContestService contestService,
            IJudgeService judgeService,
            ILanguageService languageService,
            IFileService fileService,
            IVoteService voteService,
            WebHostDbContext dbContext)
        {
            this.userManager = userManager;
            this.problemService = problemService;
            this.contestService = contestService;
            this.judgeService = judgeService;
            this.languageService = languageService;
            this.fileService = fileService;
            this.voteService = voteService;
            this.dbContext = dbContext;
        }

        private static readonly int[] allStatus = { 0, 1, 2 };

        /// <summary>
        /// 查询题目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("list")]
        [ProducesResponseType(200)]
        public async Task<ProblemListModel> ProblemList([FromBody]ProblemListQueryModel model)
        {
            var userId = userManager.GetUserId(User);

            var ret = new ProblemListModel();

            // use an invalid value when userId is empty or null
            var judges = await judgeService.QueryJudgesAsync(string.IsNullOrEmpty(userId) ? "-1" : userId, model.GroupId == 0 ? null : (int?)model.GroupId, model.ContestId == 0 ? null : (int?)model.ContestId);

            IQueryable<Problem> problems;

            try
            {
                problems = await (model switch
                {
                    { ContestId: 0, GroupId: 0 } => problemService.QueryProblemAsync(userId),
                    { GroupId: 0 } => problemService.QueryProblemAsync(userId, model.ContestId),
                    _ => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId)
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
                ret.Problems = await problems.Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
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
                var contest = await contestService.GetContestAsync(model.ContestId);
                if (contest != null && DateTime.Now < contest.StartTime)
                {
                    ret.Problems = new List<ProblemListModel.ProblemListItemModel>();
                }
                else ret.Problems = await problems.Take(model.Count).Select(i => new ProblemListModel.ProblemListItemModel
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

        /// <summary>
        /// 获取题目详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("details")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
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
                    _ => problemService.QueryProblemAsync(userId, model.ContestId, model.GroupId)
                });
            }
            catch (Exception ex)
            {
                throw new InterfaceException((HttpStatusCode)ex.HResult, ex.Message);
            }

            var problem = await problems.Where(i => i.Id == model.ProblemId).FirstOrDefaultAsync();
            if (problem is null) throw new NotFoundException("找不到该题目");

            // use an invalid value when userId is empty or null
            var judges = await judgeService.QueryJudgesAsync(string.IsNullOrEmpty(userId) ? "-1" : userId, model.GroupId == 0 ? null : (int?)model.GroupId, model.ContestId == 0 ? null : (int?)model.ContestId);

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

            ret.Name = problem.Name;
            ret.Hidden = problem.Hidden;
            ret.Level = problem.Level;
            ret.Type = problem.Type;
            ret.UserId = problem.UserId;
            ret.UserName = problem.UserInfo.UserName;
            ret.Id = problem.Id;
            ret.Description = problem.Description;
            ret.CreationTime = problem.CreationTime;
            ret.Upvote = problem.Upvote;
            ret.Downvote = problem.Downvote;
            var vote = await voteService.GetVoteAsync(userId, model.ProblemId, null);
            ret.MyVote = vote?.VoteType ?? 0;

            var config = problem.Config.DeserializeJson<ProblemConfig>(false);

            var useDefaultDisabledConfig = false;

            var langConfig = (await languageService.GetLanguageConfigAsync()).ToList();
            var langs = config.Languages?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            if (langs.Length == 0) langs = langConfig.Select(i => i.Name).ToArray();
            else useDefaultDisabledConfig = true;

            if (model.ContestId != 0)
            {
                var contest = await contestService.GetContestAsync(model.ContestId);
                if (contest != null)
                {
                    var contestConfig = contest.Config.DeserializeJson<ContestConfig>(false);
                    var contestLangs = contestConfig.Languages?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    if (contestLangs.Length != 0)
                    {
                        langs = langs.Intersect(contestLangs).ToArray();
                        useDefaultDisabledConfig = true;
                    }
                }
            }

            if (!useDefaultDisabledConfig) langs = langs.Where(i => langConfig.Any(j => j.Name == i && !j.DisabledByDefault)).ToArray();

            // For older version compatibility
            if (config.SourceFiles.Count == 0)
            {
                config.SourceFiles.Add(string.IsNullOrEmpty(config.SubmitFileName) ? "${random}${extension}" : $"{config.SubmitFileName}${{extension}}");
            }
            ret.Sources = config.SourceFiles;
            ret.Languages = LanguageConfigHelper.GenerateLanguageConfig(langConfig, langs).ToList();

            return ret;
        }

        /// <summary>
        /// 删除题目
        /// </summary>
        /// <param name="problemId"></param>
        /// <returns></returns>
        [HttpDelete]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
        public Task RemoveProblem(int problemId)
        {
            return problemService.RemoveProblemAsync(problemId);
        }

        /// <summary>
        /// 创建题目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
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

        /// <summary>
        /// 更新题目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
        public async Task UpdateProblem([FromBody]ProblemEditModel model)
        {
            var problem = await problemService.GetProblemAsync(model.Id);
            if (problem is null) throw new NotFoundException("找不到该题目");

            problem.Description = model.Description;
            problem.Hidden = model.Hidden;
            problem.Level = model.Level;
            problem.Name = model.Name;
            problem.Type = model.Type;
            problem.Config = model.Config.SerializeJsonAsString(false);

            await problemService.UpdateProblemAsync(problem);
        }

        /// <summary>
        /// 获取题目和题目配置
        /// </summary>
        /// <param name="problemId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequireTeacher]
        [Route("edit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
        public async Task<ProblemEditModel> GetProblem(int problemId)
        {
            var problem = await problemService.GetProblemAsync(problemId);
            if (problem is null) throw new NotFoundException("找不到该题目");

            var model = new ProblemEditModel
            {
                Description = problem.Description,
                Hidden = problem.Hidden,
                Id = problem.Id,
                Level = problem.Level,
                Name = problem.Name,
                Type = problem.Type,
                Config = problem.Config.DeserializeJson<ProblemConfig>(false)
            };

            // For older version compatibility
            if (model.Config.SourceFiles.Count == 0)
            {
                model.Config.SourceFiles.Add(string.IsNullOrEmpty(model.Config.SubmitFileName) ? "${random}${extension}" : $"{model.Config.SubmitFileName}${{extension}}");
            }

            return model;
        }

        /// <summary>
        /// 上传题目数据
        /// </summary>
        /// <param name="problemId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPut]
        [RequireTeacher]
        [Route("data")]
        [RequestSizeLimit(135000000)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ErrorModel))]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
        public async Task<ProblemDataUploadModel> UploadData([FromForm]int problemId, IFormFile file)
        {
            if ((await problemService.GetProblemAsync(problemId)) is null) throw new NotFoundException("找不到该题目");
            if (file.ContentType != "application/x-zip-compressed" && file.ContentType != "application/zip") throw new BadRequestException("文件格式不正确");
            if (file.Length > 134217728) throw new BadRequestException("文件大小不能超过 128 Mb");

            await using var stream = file.OpenReadStream();
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read, false, Encoding.UTF8);
            var list = new List<UploadInfo>();
            var failedList = new ProblemDataUploadModel();
            long size = 0;
            foreach (var i in zip.Entries.Where(i => !i.FullName.EndsWith("/")))
            {
                await using var entryStream = i.Open();
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

        /// <summary>
        /// 获取题目数据
        /// </summary>
        /// <param name="problemId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequireTeacher]
        [Route("data")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
        public async Task<IActionResult> GetData(int problemId)
        {
            if ((await problemService.GetProblemAsync(problemId)) is null) throw new NotFoundException("找不到该题目");

            var files = await fileService.ListFilesAsync($"Data/{problemId}/");
            var downloadedFiles = fileService.DownloadFilesAsync(files);
            var stream = new FileStream(Path.GetTempFileName(),
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None,
                4096,
                FileOptions.Asynchronous | FileOptions.DeleteOnClose | FileOptions.SequentialScan);
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                await foreach (var i in downloadedFiles)
                {
                    var entry = zip.CreateEntry(i.FileName);
                    await using var entryStream = entry.Open();
                    i.Content.WriteTo(entryStream);
                }
            }
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/x-zip-compressed", $"Data_{problemId}_{DateTime.Now:yyyyMMddHHmmssffff}.zip", true);
        }

        /// <summary>
        /// 获取题目数据文件列表
        /// </summary>
        /// <param name="problemId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequireTeacher]
        [Route("data-view")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(ErrorModel))]
        public async Task<IActionResult> GetDataFileList(int problemId)
        {
            if ((await problemService.GetProblemAsync(problemId)) is null) throw new NotFoundException("找不到该题目");

            var files = await fileService.ListFilesAsync($"Data/{problemId}/");
            var length = $"Data/{problemId}/".Length;
            var dataList = files.Select(i => $"${{datadir}}/{i[length..]}").OrderBy(i => i).ToList();
            var sb = new StringBuilder();
            foreach (var i in dataList) sb.AppendLine(i);
            return Content(sb.ToString());
        }

        /// <summary>
        /// 删除题目数据
        /// </summary>
        /// <param name="problemId"></param>
        /// <returns></returns>
        [HttpDelete]
        [RequireTeacher]
        [Route("data")]
        [ProducesResponseType(200)]
        public async Task DeleteData(int problemId)
        {
            if ((await problemService.GetProblemAsync(problemId)) is null) return;

            var files = await fileService.ListFilesAsync($"Data/{problemId}/");
            await fileService.DeleteFilesAsync(files);
        }
    }
}