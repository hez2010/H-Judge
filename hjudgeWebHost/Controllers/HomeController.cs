using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Test()
        {
            return Json("{test: 3}");
        }
    }
}
