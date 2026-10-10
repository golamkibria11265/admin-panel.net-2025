using Bll.Services.ApService;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    // Sign-in is enforced globally by CheckSessionWithRedirect (see Program.cs).
    public class RoleController : Controller
    {
        private readonly IRoleService _iRoleService;
        private readonly Serilog.ILogger _logger;
        public RoleController(IRoleService iRoleService, Serilog.ILogger logger)
        {
            _iRoleService = iRoleService;
            _logger = logger;
        }

        [HasPermission]
        public IActionResult Index()
        {
            return View();
        }
    }
}
