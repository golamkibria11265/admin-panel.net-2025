using Bll.Services.ApService;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.v1
{
    public class RoleController : Controller
    {
        private readonly IRoleService _iRoleService;
        private readonly Serilog.ILogger _logger;
        public RoleController(IRoleService iRoleService, Serilog.ILogger logger)
        {
            _iRoleService = iRoleService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
