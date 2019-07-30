using hjudge.WebHost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace hjudge.WebHost.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : ControllerBase
    {
        [AllowAnonymous]
        public IActionResult Error()
        {
            var ret = new ResultModel
            {
                ErrorCode = (ErrorDescription)HttpContext.Response.StatusCode
            };
            ret.ErrorMessage += $" ({Activity.Current?.Id ?? HttpContext.TraceIdentifier})";
            return new JsonResult(ret);
        }
    }
}
