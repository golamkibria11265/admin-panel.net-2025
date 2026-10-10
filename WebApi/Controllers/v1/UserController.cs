using Bll.Services.ApService;
using Bll.Services.CommonService;
using Dll.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utility;
using Utility.Exceptions;
using Utility.Models.ResponseModels;
using WebApi.Extensions;
using WebApi.Filters;

namespace WebApi.Controllers.v1
{
    // Sign-in is enforced globally by CheckSessionWithRedirect (see Program.cs); only the
    // actions marked [AllowAnonymous] can be used without signing in.
    public class UserController : Controller
    {
        private const string InvalidCredentialsMessage = "Invalid user name or password.";
        private readonly IUserService _userService;
        private readonly IAccessControlService _accessControlService;
        private readonly ILoginAttemptService _loginAttemptService;
        private readonly Serilog.ILogger _logger;

        public UserController
        (
            IUserService userService,
            IAccessControlService accessControlService,
            ILoginAttemptService loginAttemptService,
            Serilog.ILogger logger
        )
        {
            _userService = userService;
            _accessControlService = accessControlService;
            _loginAttemptService = loginAttemptService;
            _logger = logger;
        }

        [HasPermission]
        public IActionResult Index()
        {
            return View();
        }

        // Only administrators (role permission "User/SignUp") can create accounts.
        [HasPermission]
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HasPermission]
        [HttpPost]
        public async Task<ActionResult> SignUp(SignUpViewModel model)
        {
            ActionResponse actionResponse = new();
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    actionResponse.code = StatusCodes.Status400BadRequest;
                    actionResponse.messages = errors.Count > 0 ? errors : new List<string> { "Please check the form." };
                    return Json(actionResponse);
                }
                string userName = model.UserName!.Trim();
                await _userService.IsUsernameTaken(userName); // throws CustomApplicationException when taken

                long currentUserId = HttpContext.GetSessionUserId();
                UserViewModel userViewModel = new()
                {
                    UserName = userName,
                    Email = model.Email,
                    Password = model.Password,
                    Mobile = model.Mobile,
                    CreatedBy = currentUserId <= int.MaxValue ? (int)currentUserId : null
                };
                ApiResponse response = await _userService.AddUser(userViewModel);
                if (response.code == StatusCodes.Status200OK)
                {
                    _logger.Information("User {NewUserName} created by user {UserId}", userName, currentUserId);
                    actionResponse.code = StatusCodes.Status200OK;
                    actionResponse.messages = new List<string>() { "User created successfully. Assign a role before the user can sign in." };
                    return Json(actionResponse);
                }
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = new List<string>() { "Something went wrong" };
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in UserController/SignUp");
                return Json(actionResponse);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.GetSessionUserId() > 0)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(LogInViewModel login)
        {
            ActionResponse actionResponse = new();
            string userName = (login.userName ?? string.Empty).Trim();
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.Information("Login attempt for user {UserName} from {IpAddress}", userName, ipAddress);

            // Start every login from an empty session, so nothing from a previous user or a failed attempt survives.
            HttpContext.Session.Clear();
            try
            {
                if (!ModelState.IsValid)
                {
                    actionResponse.code = StatusCodes.Status400BadRequest;
                    actionResponse.messages = new List<string> { "Please enter your user name and password." };
                    return Json(actionResponse);
                }
                if (_loginAttemptService.IsLockedOut(userName, ipAddress))
                {
                    _logger.Warning("Login blocked (too many failed attempts) for user {UserName} from {IpAddress}", userName, ipAddress);
                    actionResponse.code = StatusCodes.Status429TooManyRequests;
                    actionResponse.messages = new List<string>
                    {
                        $"Too many failed attempts. Please try again in {(int)_loginAttemptService.LockoutDuration.TotalMinutes} minutes or contact the administrator."
                    };
                    return Json(actionResponse);
                }

                var user = await _userService.ValidateCredentialsAsync(userName, login.password ?? string.Empty);
                if (user == null)
                {
                    _loginAttemptService.RegisterFailure(userName, ipAddress);
                    _logger.Warning("Failed login for user {UserName} from {IpAddress}", userName, ipAddress);
                    actionResponse.code = StatusCodes.Status401Unauthorized;
                    actionResponse.messages = new List<string> { InvalidCredentialsMessage };
                    return Json(actionResponse);
                }
                _loginAttemptService.RegisterSuccess(userName);

                ApiResponse roleDataByUserName = await _userService.getRoleByUserName(user.UserName ?? userName);
                string? roleName = roleDataByUserName.data?.ToString();
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    throw new CustomApplicationException("Role not found, Please contact the administrator");
                }

                var permissions = (await _accessControlService.GetPermissionsAsync(user.Id)).ToList();
                ApiResponse nestedMenu = _userService.GetUserNestedMenuList(permissions);
                var nestedMenuList = nestedMenu.data as List<NestedMenuInfoViewModel> ?? new List<NestedMenuInfoViewModel>();

                // Session values used by the layout (user name, sidebar menu). Authorization itself is always
                // re-checked against the database by CheckSessionWithRedirect / HasPermission.
                HttpContext.Session.SetString(SessionKeys.UserName, user.UserName ?? userName);
                HttpContext.Session.SetString(SessionKeys.RoleName, roleName);
                HttpContext.Session.SetString(SessionKeys.RoleWisePermission, JsonSerializer.Serialize(permissions));
                HttpContext.Session.SetString(SessionKeys.RoleWiseMenuPermission, JsonSerializer.Serialize(nestedMenuList));
                // Set LAST: from here on the user counts as signed in.
                HttpContext.Session.SetString(SessionKeys.UserId, user.Id.ToString());

                _logger.Information("User {UserId} ({UserName}) signed in from {IpAddress}", user.Id, user.UserName, ipAddress);
                actionResponse.code = StatusCodes.Status200OK;
                actionResponse.messages = new List<string>() { "Successfully logged In" };
                actionResponse.redirect_url = Url.Action("Index", "Home");
                return Json(actionResponse);
            }
            catch (Exception ex)
            {
                HttpContext.Session.Clear(); // never leave a half-signed-in session behind
                actionResponse.code = StatusCodes.Status417ExpectationFailed;
                actionResponse.messages = SafeErrorMessage.For(ex);
                _logger.Error(ex, "Error in UserController/Login");
                return Json(actionResponse);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                long userId = HttpContext.GetSessionUserId();
                if (userId > 0)
                {
                    _logger.Information("User {UserId} ({UserName}) logged out", userId, HttpContext.Session.GetString(SessionKeys.UserName));
                }
                HttpContext.Session.Clear();
                Response.Cookies.Delete(".AdminPanel.Session");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UserController/Logout");
            }
            return RedirectToAction("Login");
        }

        /// <summary>Lets the page find out whether the session is still valid. Returns sessionExpired = true when it is NOT.</summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult CheckSession()
        {
            bool sessionExpired = HttpContext.GetSessionUserId() <= 0;
            return Json(new { sessionExpired });
        }
    }
}
