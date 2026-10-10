using Bll.Services.ApService;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    // Sign-in is enforced globally by CheckSessionWithRedirect (see Program.cs).
    public class UserRoleController : Controller
    {
        private readonly IUserRoleService _iUserRoleService;
        private readonly Serilog.ILogger _logger;
        public UserRoleController(IUserRoleService iUserRoleService, Serilog.ILogger logger)
        {
            _iUserRoleService = iUserRoleService;
            _logger = logger;
        }

        [HasPermission]
        public IActionResult Index()
        {
            return View();
        }
    }
}
