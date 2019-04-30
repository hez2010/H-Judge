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
    public class ApiController : ControllerBase
    {
        private readonly IJudgeService judgeService;

        public ApiController(IJudgeService judgeService)
        {
            this.judgeService = judgeService;
        }

    }
}
