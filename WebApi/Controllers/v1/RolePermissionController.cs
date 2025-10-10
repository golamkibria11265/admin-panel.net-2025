using Microsoft.AspNetCore.Mvc;
using Utility.Models.ResponseModels;
using Utility;
using Dll.Models.ViewModels;
using WebApi.Filters;
using Bll.Services.ApService;

namespace WebApi.Controllers.v1
{
    public class RolePermissionController : Controller
    {
        private readonly IRolePermissionService _iRolePermissionService;
        private readonly Serilog.ILogger _logger;
        public RolePermissionController(IRolePermissionService iRolePermissionService, Serilog.ILogger logger)
        {
            _iRolePermissionService = iRolePermissionService;
            _logger = logger;
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        public async Task<IActionResult> Index()
        {
            ApiResponse apiResponse = await _iRolePermissionService.GetRolePermissionsByRoleId(0);
            var result = apiResponse.data as RolePermissionNestedViewModel;
            return View(result);
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [HttpPost]
        public async Task<IActionResult> GetRolePermissionsByRoleId(int roleId)
        {
            ActionResponse actionResponse = new();
            try
            {
                ApiResponse apiResponse = await _iRolePermissionService.GetRolePermissionsByRoleId(roleId);
                var result = apiResponse.data as RolePermissionNestedViewModel;
                actionResponse.code = StatusCodes.Status200OK;
                actionResponse.messages = new List<string> { "Successfully fetched" };
                actionResponse.data = result;
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in RolePermissionController/GetRolePermissionsByRoleId");
                return Json(actionResponse);
            }
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [HttpPost]
        public async Task<IActionResult> UpdateRolePermissionsByRole(RolePermissionNestedViewModel rolePermissionNestedViewModel)
        {
            ActionResponse actionResponse = new();
            try
            {
                ApiResponse apiResponse = await _iRolePermissionService.UpdateRolePermissionsByRoleId(rolePermissionNestedViewModel);
                return Json(apiResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in RolePermissionController/UpdateRolePermissionsByRole");
                return Json(actionResponse);
            }
        }
    }
}
