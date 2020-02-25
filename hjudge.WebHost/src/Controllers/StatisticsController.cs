using System;
using System.Linq;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor;
using hjudge.Core;
using hjudge.WebHost.Data;
using hjudge.WebHost.Data.Identity;
using hjudge.WebHost.Models.Statistics;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IJudgeService judgeService;
        private readonly UserManager<UserInfo> userManager;

        public StatisticsController(IJudgeService judgeService, UserManager<UserInfo> userManager)
        {
            this.judgeService = judgeService;
            this.userManager = userManager;
        }

        /// <summary>
        /// 查询提交状态
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("list")]
        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<StatisticsListModel> StatisticsList([FromBody]StatisticsListQueryModel model)
        {
            int? resultType = null;
            if (!string.IsNullOrEmpty(model.Result) && model.Result != "All")
            {
                if (Enum.TryParse<ResultCode>(model.Result.Trim(), true, out var type)) resultType = (int?)type;
            }

            string? queryUserId = null;
            if (!string.IsNullOrEmpty(model.UserName))
            {
                var queryUser = await userManager.FindByNameAsync(model.UserName);
                queryUserId = queryUser?.Id ?? "-1";
            }

            var judges = await judgeService.QueryJudgesAsync(queryUserId, model.GroupId, model.ContestId, model.ProblemId, resultType);

            IQueryable<Judge> query = judges;

            var ret = new StatisticsListModel();

            if (model.RequireTotalCount) ret.TotalCount = await query.Select(i => i.Id).Cacheable().CountAsync();

            query = query.OrderByDescending(i => i.Id);

            if (model.StartId != 0) query = query.Where(i => i.Id <= model.StartId);
            else query = query.Skip(model.Start);

            ret.Statistics = await query.Take(model.Count).Select(i => new StatisticsListModel.StatisticsListItemModel
            {
                ContestId = i.ContestId,
                GroupId = i.GroupId,
                ProblemId = i.ProblemId,
                ResultId = i.Id,
                ResultType = i.ResultType,
                Time = i.JudgeTime,
                UserId = i.UserId,
                UserName = i.UserInfo.UserName,
                ProblemName = i.Problem.Name,
                Language = i.Language,
                Score = i.FullScore
            }).Cacheable().ToListAsync();

            return ret;
        }
    }
}
