using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hjudgeCore;
using hjudgeWebHost.Models;
using hjudgeWebHost.Services;
using Microsoft.AspNetCore.Mvc;

namespace hjudgeWebHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JudgeReportController : ControllerBase
    {
        private readonly IJudgeService judgeService;

        public JudgeReportController(IJudgeService judgeService)
        {
            this.judgeService = judgeService;
        }

        [HttpPost]
        public async Task<ResultModel> ReportResult([FromBody]JudgeResult result)
        {
            var ret = new ResultModel();
            
        }
    }
}
