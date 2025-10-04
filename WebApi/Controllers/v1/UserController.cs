using Bll.Services.ApService;
using Dll.Data.Entities;
using Dll.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utility;
using Utility.Exceptions;
using Utility.Models.ResponseModels;

namespace WebApi.Controllers.v1
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly Serilog.ILogger _logger;
        private readonly IHttpContextAccessor _iHttpContextAccesor;
        private readonly IConfiguration _iConfiguration;

        public UserController
        (
            IUserService userService,
            IHttpContextAccessor iHttpContextAccesor,
            Serilog.ILogger logger,
            IConfiguration iConfiguration
        )
        {
            _userService = userService;
            _logger = logger;
            _iHttpContextAccesor = iHttpContextAccesor;
            _iConfiguration = iConfiguration;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SignUp(SignUpViewModel model)
        {
            ActionResponse actionResponse = new();
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new CustomApplicationException("Model is not valid", StatusCodes.Status400BadRequest);
                }
                ApiResponse result = await _userService.IsUsernameTaken(model.UserName ?? string.Empty);
                if (result.code != StatusCodes.Status200OK)
                {
                    throw new CustomApplicationException("This user name is taken");
                }
                UserViewModel userViewModel = new UserViewModel();
                userViewModel.UserName = model.UserName;
                userViewModel.Email = model.Email;
                userViewModel.Password = model.Password;
                userViewModel.Mobile = model.Mobile;
                ApiResponse response = await _userService.AddUser(userViewModel);
                if(response.code == StatusCodes.Status200OK)
                {
                    actionResponse.code = StatusCodes.Status200OK;
                    actionResponse.messages = new List<string>() { "You are registered successfully. Please log in" };
                    return Json(actionResponse);
                }
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { "Something went wrong" };
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in UserController/SignUp");
                return Json(actionResponse);
            }
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(LogInViewModel login)
        {
            ActionResponse actionResponse = new();
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new CustomApplicationException("Model is not valid", StatusCodes.Status400BadRequest);
                }
                var httpContext = _iHttpContextAccesor.HttpContext;
                var isFirstAdminExist = await _userService.IsFirstAdminExist(login.userName ?? string.Empty, login.password ?? string.Empty);
                if (httpContext == null)
                {
                    throw new CustomApplicationException("HttpContext not found, Please contact the administrator");
                }
                if (isFirstAdminExist.code != StatusCodes.Status200OK)
                {
                    throw new CustomApplicationException("Role not found, Please contact the administrator");
                }
                ApiResponse roleDataByUserName = await _userService.getRoleByUserName(login.userName ?? string.Empty);
                if (roleDataByUserName.data == null)
                {
                    throw new CustomApplicationException("User role is not found");
                }
                string userName = login.userName ?? string.Empty;
                string? roleName = roleDataByUserName.data.ToString();
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    throw new CustomApplicationException("Role not found, Please contact the administrator");
                }
                httpContext.Session.SetString("UserName", userName);
                httpContext.Session.SetString("RoleName", roleName);
                ApiResponse userRoleWisePermissionInfo = await _userService.GetUserRoleWisePermissionList(roleName);
                if (userRoleWisePermissionInfo.data == null)
                {
                    throw new CustomApplicationException("User permission is are not found");
                }
                List<PermissionInfoViewModel>? userRoleWisePermissionInfoList = userRoleWisePermissionInfo.data as List<PermissionInfoViewModel>;
                if (userRoleWisePermissionInfoList == null)
                {
                    throw new CustomApplicationException("User role list is not found");
                }
                httpContext.Session.SetString("RoleWisePermission", JsonSerializer.Serialize(userRoleWisePermissionInfoList));

                ApiResponse userRoleWiseNestedMenuList = _userService.GetUserNestedMenuList(userRoleWisePermissionInfoList);
                if (userRoleWiseNestedMenuList == null)
                {
                    throw new CustomApplicationException("User nested menu list is not found");
                }
                List<NestedMenuInfoViewModel>? userRoleWiseNestedMenuInfoList = userRoleWiseNestedMenuList.data as List<NestedMenuInfoViewModel>;
                httpContext.Session.SetString("RoleWiseMenuPermission", JsonSerializer.Serialize(userRoleWiseNestedMenuInfoList));

                string? role_name = httpContext.Session.GetString("RoleName");
                string? user_name = httpContext.Session.GetString("UserName");

                ViewBag.userName = user_name;
                ViewBag.roleName = role_name;

                ApiResponse currentUserInfo = await _userService.GetCurrentUserInfo();
                if (currentUserInfo.data == null)
                {
                    throw new CustomApplicationException("Current user is not found");
                }
                ApUser? user = currentUserInfo.data as ApUser;
                if (user == null)
                {
                    throw new CustomApplicationException("Current user is not found");
                }
                httpContext.Session.SetString("Id", user.Id.ToString());
                if (userRoleWisePermissionInfoList.Any(a => a.ControllerName!.ToLower() == "Home".ToLower() && a.ActionName!.ToLower() == "Index".ToLower()))
                {
                    actionResponse.code = StatusCodes.Status200OK;
                    actionResponse.messages = new List<string>() { "Successfully logged In" };
                    actionResponse.redirect_url = _iConfiguration.GetValue<string>("Urls:LoginRedirectUrl");
                    return Json(actionResponse);
                }
                actionResponse.code = StatusCodes.Status200OK;
                actionResponse.messages = new List<string>() { "Successfully logged In" };
                actionResponse.redirect_url = _iConfiguration.GetValue<string>("Urls:LoginRedirectUrl");
                return Json(actionResponse);
            }
            catch(Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { ex.Message };
                _logger.Error(ex, "Error in UserController/Login");
                return Json(actionResponse);
            }
        }
        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                var sessionInfo = _iHttpContextAccesor.HttpContext?.Session.Keys.ToList();
                string client_id = string.Empty;
                string remarks = string.Empty;
                string roleName = string.Empty;
                if (sessionInfo != null && sessionInfo.Count > 0)
                {
                    client_id = _iHttpContextAccesor.HttpContext?.Session.GetString("UserName")!;
                    roleName = _iHttpContextAccesor.HttpContext?.Session.GetString("RoleName")!;
                    remarks = "Logged out successfully by user: " + client_id + " || role: " + roleName + "";
                    _logger.Information(remarks);
                }
                _iHttpContextAccesor.HttpContext?.Session.SetString("UserName", "");
                _iHttpContextAccesor.HttpContext?.Session.SetString("RoleName", "");
                _iHttpContextAccesor.HttpContext?.Session.Clear();
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                return RedirectToAction("Login");
            }
        }
        public IActionResult CheckSession()
        {
            var userName = _iHttpContextAccesor.HttpContext?.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(userName))
            {
                return Json(new { sessionExpired = true });
            }
            return Json(new { sessionExpired = false });
        }
    }
}
