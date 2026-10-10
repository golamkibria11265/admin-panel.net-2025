using Bll.Services.CommonService;
using Microsoft.AspNetCore.Mvc;
using Utility;
using Utility.Models.ResponseModels;
using WebApi.Extensions;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    // Sign-in is enforced globally by CheckSessionWithRedirect (see Program.cs).
    public class CommonController : Controller
    {
        private readonly ICommonService _iCommonService;
        private readonly Serilog.ILogger _logger;
        public CommonController(ICommonService iCommonService, Serilog.ILogger logger)
        {
            _iCommonService = iCommonService;
            _logger = logger;
        }

        [HasPermission("RolePermission/Index")]
        public async Task<IActionResult> GetRoleList()
        {
            ActionResponse actionResponse = new();
            try
            {
                ApiResponse apiResponse = await _iCommonService.GetRoleList();
                return Json(apiResponse.data);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in CommonController/GetRoleList");
                return Json(actionResponse);
            }
        }
    }
}
