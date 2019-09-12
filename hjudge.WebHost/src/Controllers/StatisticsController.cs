using hjudge.WebHost.Models.Statistics;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using Microsoft.EntityFrameworkCore;
using hjudge.WebHost.Data;
using hjudge.Core;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Route("statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IJudgeService judgeService;

        public StatisticsController(IJudgeService judgeService)
        {
            this.judgeService = judgeService;
        }

        enum LastNamingState
        {
            Start, Blank, Other
        }

        [Route("list")]
        [HttpPost]
        public async Task<StatisticsListModel> StatisticsList([FromBody]StatisticsListQueryModel model)
        {
            int? resultType = null;
            if (!string.IsNullOrEmpty(model.Result) && model.Result != "All")
            {
                if (Enum.TryParse<ResultCode>(model.Result.Trim(), true, out var type)) resultType = (int?)type;
            }
            var judges = await judgeService.QueryJudgesAsync(model.UserId, model.GroupId, model.ContestId, model.ProblemId, resultType);

            IQueryable<Judge> query = judges;

            var ret = new StatisticsListModel();

            if (model.RequireTotalCount) ret.TotalCount = await query.Select(i => i.Id)/*.Cacheable()*/.CountAsync();

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
            })/*.Cacheable()*/.ToListAsync();

            return ret;
        }
    }
}
