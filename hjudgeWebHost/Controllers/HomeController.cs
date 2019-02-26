using hjudgeWebHost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Controllers
{
    public class HomeController : ControllerBase
    {
        [AllowAnonymous]
        public IActionResult Error()
        {
            var ret = new ResultModel
            {
                ErrorCode = ErrorDescription.InteralServerException
            };
            ret.ErrorMessage += $" ({Activity.Current?.Id ?? HttpContext.TraceIdentifier})";
            return new JsonResult(ret);
        }
    }
}
