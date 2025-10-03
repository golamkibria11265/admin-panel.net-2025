using Bll.Services.ApService;
using Microsoft.AspNetCore.Mvc;
using Utility;
using Utility.Models.ResponseModels;

namespace WebApi.Controllers.v1
{
    public class PermissionController : Controller
    {
        private readonly IPermissionService _iPermissionService;
        private readonly Serilog.ILogger _logger;
        public PermissionController(IPermissionService iPermissionService, Serilog.ILogger logger)
        {
            _iPermissionService = iPermissionService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
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
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in PermissionController/Sync");
                return Json(actionResponse);
            }
        }
    }
}
