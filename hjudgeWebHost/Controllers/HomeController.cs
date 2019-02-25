using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
    public class HomeController : ControllerBase
    {
        public IActionResult Test()
        {
            return new JsonResult("{test: 3}");
        }
    }
}
