using hjudgeWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace hjudgeWeb.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return Json(new ResultModel
            {
                IsSucceeded = false,
                ErrorMessage = $"Error. {Activity.Current?.Id ?? HttpContext.TraceIdentifier}"
            });
        }
    }
}
