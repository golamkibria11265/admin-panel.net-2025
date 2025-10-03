using Bll.Services.ApService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.v1
{
    public class UserRoleController : Controller
    {
        private readonly IUserRoleService _iUserRoleService;
        private readonly Serilog.ILogger _logger;
        public UserRoleController(IUserRoleService iUserRoleService, Serilog.ILogger logger)
        {
            _iUserRoleService = iUserRoleService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
