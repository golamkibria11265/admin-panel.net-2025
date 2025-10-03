using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApi.Filters;
using WebApi.Models;

namespace WebApi.Controllers.v1
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        public IActionResult Index()
        {
            return View();
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [TypeFilter(typeof(HasPermission), Order = 2)]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
