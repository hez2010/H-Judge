using hjudge.WebHost.Models.Statistics;
using hjudge.WebHost.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudge.WebHost.Controllers
{
    [Route("statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IJudgeService judgeService;

        public StatisticsController(IJudgeService judgeService)
        {
            this.judgeService = judgeService;
        }

        [Route("list")]
        [HttpPost]
        public async Task<StatisticsListModel> StatisticsList([FromBody]StatisticsListQueryModel model)
        {
            judgeService.QueryJudgesAsync
        }
    }
}
