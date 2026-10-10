using Bll.Services.ApService;
using Dll.Models.RequestModels;
using Dll.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Utility;
using Utility.Exceptions;
using Utility.Models.ResponseModels;
using WebApi.Extensions;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    // Sign-in is enforced globally by CheckSessionWithRedirect (see Program.cs).
    public class MenuController : Controller
    {
        private const int MaxPageSize = 50; // same cap as PaginationParamsRequest
        private readonly IMenuService _menuService;
        private readonly IAccessControlService _accessControlService;
        private readonly Serilog.ILogger _logger;

        public MenuController(IMenuService menuService, IAccessControlService accessControlService, Serilog.ILogger logger)
        {
            _menuService = menuService;
            _accessControlService = accessControlService;
            _logger = logger;
        }

        [HasPermission]
        public async Task<IActionResult> Index()
        {
            long userId = HttpContext.GetSessionUserId();
            // Only used to show/hide buttons; every action is checked again on the server.
            ViewData["addPermission"] = await _accessControlService.HasPermissionAsync(userId, "Menu", "Add") ? "Y" : "N";
            ViewData["editPermission"] = await _accessControlService.HasPermissionAsync(userId, "Menu", "Edit") ? "Y" : "N";
            ViewData["deletePermission"] = await _accessControlService.HasPermissionAsync(userId, "Menu", "Delete") ? "Y" : "N";
            return View();
        }

        [HasPermission("Menu/Index")]
        [HttpPost, ActionName("PaginatedMenus")]
        public async Task<IActionResult> PaginatedMenus()
        {
            ActionResponse actionResponse = new();
            try
            {
                int start = ParseInt(Request.Form["start"].FirstOrDefault(), 0);
                int length = ParseInt(Request.Form["length"].FirstOrDefault(), 10);
                if (start < 0) start = 0;
                if (length <= 0 || length > MaxPageSize) length = length <= 0 ? 10 : MaxPageSize; // -1 ("All") or 0 -> safe page size

                MenuParamsRequest menuParamsRequest = new()
                {
                    PageNumber = (start / length) + 1,
                    PageSize = length,
                    SearchString = Request.Form["search[value]"].FirstOrDefault(),
                    Parent = Request.Form["id"].FirstOrDefault()
                };
                ApiResponse apiResponse = await _menuService.GetMenuList(menuParamsRequest);
                return new JsonResult(new
                {
                    draw = ParseInt(Request.Form["draw"].FirstOrDefault(), 0),
                    recordsTotal = apiResponse.TotalCount,
                    recordsFiltered = apiResponse.TotalCount,
                    data = apiResponse.data
                });
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in MenuController/PaginatedMenus");
                return Json(actionResponse);
            }
        }

        [HasPermission]
        [HttpGet]
        public async Task<IActionResult> Add(int id = 0, int parent = 0, string parentName = "")
        {
            return await LoadMenuForm(id, parent, parentName);
        }

        [HasPermission]
        [HttpGet]
        public async Task<IActionResult> Edit(int id = 0, int parent = 0, string parentName = "")
        {
            return await LoadMenuForm(id, parent, parentName);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEdit(MenuViewModel menuViewModel)
        {
            ActionResponse actionResponse = new();
            try
            {
                // Adding and editing are separate permissions.
                string requiredAction = menuViewModel.Id > 0 ? "Edit" : "Add";
                if (!await _accessControlService.HasPermissionAsync(HttpContext.GetSessionUserId(), "Menu", requiredAction))
                {
                    return HasPermissionFilter.DeniedResult(HttpContext);
                }
                if (!ModelState.IsValid)
                {
                    throw new CustomApplicationException(string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }
                ApiResponse menu = await _menuService.AddOrEditMenu(menuViewModel);
                if (menu.code != StatusCodes.Status200OK)
                {
                    throw new CustomApplicationException("Failed");
                }
                actionResponse.code = menu.code;
                actionResponse.messages = menu.messages;
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in MenuController/AddOrEdit");
                return Json(actionResponse);
            }
        }

        [HasPermission]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponse actionResponse = new();
            try
            {
                ApiResponse result = await _menuService.DeleteMenuById(id);
                if (result.code != StatusCodes.Status200OK)
                {
                    throw new CustomApplicationException("Failed");
                }
                actionResponse.code = result.code;
                actionResponse.messages = result.messages;
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in MenuController/Delete");
                return Json(actionResponse);
            }
        }

        private async Task<IActionResult> LoadMenuForm(int id, int parent, string parentName)
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
                ApiResponse apiResponse = await _menuService.GetMenuById(id);
                if (apiResponse.code == StatusCodes.Status200OK && apiResponse.data is MenuViewModel loaded)
                {
                    menuViewModel = loaded;
                }
                return View(menuViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in MenuController/GetMenuById");
                return View(menuViewModel);
            }
        }

        private static int ParseInt(string? value, int fallback)
        {
            return int.TryParse(value, out var result) ? result : fallback;
        }
    }
}
