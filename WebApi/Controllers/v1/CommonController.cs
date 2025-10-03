using Microsoft.AspNetCore.Mvc;
using Utility.Models.ResponseModels;
using Utility;
using Bll.Services.CommonService;

namespace WebApi.Controllers.v1
{
    public class CommonController : Controller
    {
        private readonly ICommonService _iCommonService;
        private readonly Serilog.ILogger _logger;
        public CommonController(ICommonService iCommonService, Serilog.ILogger logger)
        {
            _iCommonService = iCommonService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetRoleList()
        {
            ActionResponse actionResponse = new();
            try
            {
                ApiResponse apiResponse = await _iCommonService.GetRoleList();
                actionResponse.code = StatusCodes.Status200OK;
                actionResponse.messages = apiResponse.messages;
                actionResponse.data = apiResponse.data;
                return Json(actionResponse.data);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in CommonControllerController/RoleList");
                return Json(actionResponse);
            }
        }
    }
}
