using Bll.Services.ApService;
using Dll.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Utility;
using Utility.Models.ResponseModels;
using WebApi.Extensions;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    // Sign-in is enforced globally by CheckSessionWithRedirect (see Program.cs).
    // Every action needs the "RolePermission/Index" permission: changing role permissions is an admin-only task.
    public class RolePermissionController : Controller
    {
        private readonly IRolePermissionService _iRolePermissionService;
        private readonly Serilog.ILogger _logger;
        public RolePermissionController(IRolePermissionService iRolePermissionService, Serilog.ILogger logger)
        {
            _iRolePermissionService = iRolePermissionService;
            _logger = logger;
        }

        [HasPermission]
        public async Task<IActionResult> Index()
        {
            try
            {
                ApiResponse apiResponse = await _iRolePermissionService.GetRolePermissionsByRoleId(0);
                var result = apiResponse.data as RolePermissionNestedViewModel ?? new RolePermissionNestedViewModel();
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in RolePermissionController/Index");
                return View(new RolePermissionNestedViewModel());
            }
        }

        [HasPermission("RolePermission/Index")]
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
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in RolePermissionController/GetRolePermissionsByRoleId");
                return Json(actionResponse);
            }
        }

        [HasPermission("RolePermission/Index")]
        [HttpPost]
        public async Task<IActionResult> UpdateRolePermissionsByRole(RolePermissionNestedViewModel rolePermissionNestedViewModel)
        {
            ActionResponse actionResponse = new();
            try
            {
                ApiResponse apiResponse = await _iRolePermissionService.UpdateRolePermissionsByRoleId(rolePermissionNestedViewModel);
                _logger.Information("Role permissions of role {RoleId} updated by user {UserId}: {Result}",
                    rolePermissionNestedViewModel?.RoleId, HttpContext.GetSessionUserId(), apiResponse.code);
                return Json(apiResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in RolePermissionController/UpdateRolePermissionsByRole");
                return Json(actionResponse);
            }
        }
    }
}
