using Bll.Services.ApService;
using Microsoft.AspNetCore.Mvc;
using Utility;
using Utility.Models.ResponseModels;
using WebApi.Extensions;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    // Sign-in is enforced globally by CheckSessionWithRedirect (see Program.cs).
    public class PermissionController : Controller
    {
        private readonly IPermissionService _iPermissionService;
        private readonly Serilog.ILogger _logger;
        public PermissionController(IPermissionService iPermissionService, Serilog.ILogger logger)
        {
            _iPermissionService = iPermissionService;
            _logger = logger;
        }

        [HasPermission]
        public IActionResult Index()
        {
            return View();
        }

        // Changes data, so POST only (protected by the global anti-forgery check).
        [HasPermission("Permission/Index")]
        [HttpPost]
        public async Task<IActionResult> SyncPermissionList()
        {
            ActionResponse actionResponse = new();
            try
            {
                ApiResponse apiResponse = await _iPermissionService.SyncPermissionList();
                actionResponse.code = StatusCodes.Status200OK;
                actionResponse.messages = apiResponse.messages;
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in PermissionController/Sync");
                return Json(actionResponse);
            }
        }
    }
}
