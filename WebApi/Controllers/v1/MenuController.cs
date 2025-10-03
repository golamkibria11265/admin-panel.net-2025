using Bll.Services.ApService;
using Dll.Models.RequestModels;
using Dll.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utility;
using Utility.Exceptions;
using Utility.Models.ResponseModels;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly Serilog.ILogger _logger;
        private readonly IHttpContextAccessor _iHttpContextAccesor;
        public MenuController(IMenuService menuService, IHttpContextAccessor iHttpContextAccesor, Serilog.ILogger logger)
        {
            _menuService = menuService;
            _logger = logger;
            _iHttpContextAccesor = iHttpContextAccesor;
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [TypeFilter(typeof(HasPermission), Order = 2)]
        public IActionResult Index()
        {
            var httpContext = _iHttpContextAccesor.HttpContext;
            if (httpContext == null)
            {
                throw new CustomApplicationException("HttpContext not found, Please contact the administrator");
            }
            var permissionString = _iHttpContextAccesor.HttpContext?.Session.GetString("RoleWisePermission");
            var roleWisePermissions = new List<PermissionInfoViewModel>();
            if (permissionString != null)
            {
                roleWisePermissions = JsonSerializer.Deserialize<List<PermissionInfoViewModel>>(permissionString ?? string.Empty);
            }
            if (roleWisePermissions != null && roleWisePermissions.Any(a => a.ControllerName!.ToLower() == "Menu".ToLower()) && roleWisePermissions.Any(a => a.ActionName!.ToLower() == "Add".ToLower()))
            {
                httpContext.Session.SetString("addPermission", "Y");
            }
            else
            {
                httpContext.Session.SetString("addPermission", "N");
            }
            if (roleWisePermissions != null && roleWisePermissions.Any(a => a.ControllerName!.ToLower() == "Menu".ToLower()) && roleWisePermissions.Any(a => a.ActionName!.ToLower() == "Edit".ToLower()))
            {
                httpContext.Session.SetString("editPermission", "Y");
            }
            else
            {
                httpContext.Session.SetString("editPermission", "N");
            }
            if (roleWisePermissions != null && roleWisePermissions.Any(a => a.ControllerName!.ToLower() == "Menu".ToLower()) && roleWisePermissions.Any(a => a.ActionName!.ToLower() == "Delete".ToLower()))
            {
                httpContext.Session.SetString("deletePermission", "Y");
            }
            else
            {
                httpContext.Session.SetString("deletePermission", "N");
            }
            return View();
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [HttpPost, ActionName("PaginatedMenus")]
        public async Task<IActionResult> PaginatedMenus()
        {
            ActionResponse actionResponse = new();
            try
            {
                MenuParamsRequest menuParamsRequest = new();
                menuParamsRequest.PageNumber = (Convert.ToInt32(Request.Form["start"].FirstOrDefault()) / Convert.ToInt32(Request.Form["length"].FirstOrDefault())) + 1;
                menuParamsRequest.PageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
                menuParamsRequest.SearchString = Request.Form["search[value]"].FirstOrDefault();
                menuParamsRequest.Parent = Request.Form["id"].FirstOrDefault();
                ApiResponse? apiResponse = await _menuService.GetMenuList(menuParamsRequest);
                if (apiResponse.data is List<MenuViewModel> list)
                {
                    actionResponse.data = list;
                    actionResponse.CurrentPage = apiResponse.CurrentPage;
                    actionResponse.PageSize = apiResponse.PageSize;
                    actionResponse.TotalCount = apiResponse.TotalCount;
                    actionResponse.TotalPages = apiResponse.TotalPages;
                }
                return new JsonResult(new
                {
                    draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault()),
                    recordsTotal = apiResponse.TotalCount,
                    recordsFiltered = apiResponse.TotalCount,
                    data = apiResponse.data
                });
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in MenuController/PaginatedMenus");
                return Json(actionResponse);
            }
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [TypeFilter(typeof(HasPermission), Order = 2)]
        [HttpGet]
        public async Task<IActionResult> Add(int id = 0, int parent = 0, string parentName = "")
        {
            MenuViewModel? menuViewModel = new();
            try
            {
                if (id == 0)
                {
                    menuViewModel.Parent = parent;
                    menuViewModel.ParentName = parentName;
                    return View(menuViewModel);
                }
                ApiResponse? apiResponse = await _menuService.GetMenuById(id);
                if (apiResponse.code == StatusCodes.Status200OK && apiResponse.data is MenuViewModel)
                {
                    menuViewModel = apiResponse.data as MenuViewModel;
                }
                return View(menuViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in MenuController/GetMenuById");
                return View(menuViewModel);
            }
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [TypeFilter(typeof(HasPermission), Order = 2)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id = 0, int parent = 0, string parentName = "")
        {
            MenuViewModel? menuViewModel = new();
            try
            {
                if (id == 0)
                {
                    menuViewModel.Parent = parent;
                    menuViewModel.ParentName = parentName;
                    return View(menuViewModel);
                }
                ApiResponse? apiResponse = await _menuService.GetMenuById(id);
                if (apiResponse.code == StatusCodes.Status200OK && apiResponse.data is MenuViewModel)
                {
                    menuViewModel = apiResponse.data as MenuViewModel;
                }
                return View(menuViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in MenuController/GetMenuById");
                return View(menuViewModel);
            }
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(MenuViewModel menuViewModel)
        {
            ActionResponse actionResponse = new();
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new CustomApplicationException("Model is not valid", StatusCodes.Status400BadRequest);
                }
                ApiResponse? menu = await _menuService.AddOrEditMenu(menuViewModel);
                if (menu.code != StatusCodes.Status200OK)
                {
                    throw new CustomApplicationException("Failed", StatusCodes.Status417ExpectationFailed);
                }
                actionResponse.code = menu.code;
                actionResponse.messages = menu.messages;
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in MenuController/AddOrEdit");
                return Json(actionResponse);
            }
        }
        [TypeFilter(typeof(CheckSessionWithRedirect), Order = 1)]
        [TypeFilter(typeof(HasPermission), Order = 2)]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponse? actionResponse = new();
            try
            {
                ApiResponse? result = await _menuService.DeleteMenuById(id);
                if (result.code != StatusCodes.Status200OK)
                {
                    throw new CustomApplicationException("Failed", StatusCodes.Status417ExpectationFailed);
                }
                actionResponse.code = result.code;
                actionResponse.messages = result.messages;
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in MenuController/Delete");
                return Json(actionResponse);
            }
        }
    }
}
