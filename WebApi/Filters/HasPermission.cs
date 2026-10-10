using Bll.Services.ApService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Utility.Models.ResponseModels;
using WebApi.Extensions;

namespace WebApi.Filters
{
    /// <summary>
    /// Requires a role permission for the current action.
    /// [HasPermission]                 -> needs the menu permission "{Controller}/{Action}" of the current action.
    /// [HasPermission("Menu/Index")]   -> needs the named permission (used for AJAX/data endpoints of a page).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class HasPermissionAttribute : TypeFilterAttribute
    {
        public HasPermissionAttribute(string permission = "") : base(typeof(HasPermissionFilter))
        {
            Arguments = new object[] { permission };
        }
    }

    public class HasPermissionFilter : IAsyncActionFilter
    {
        public const string AccessDeniedMessage = "You do not have permission to perform this action.";
        private readonly IAccessControlService _accessControlService;
        private readonly Serilog.ILogger _logger;
        private readonly string _permission;

        public HasPermissionFilter(IAccessControlService accessControlService, Serilog.ILogger logger, string permission)
        {
            _accessControlService = accessControlService;
            _logger = logger;
            _permission = permission;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string controllerName;
            string actionName;
            if (!string.IsNullOrWhiteSpace(_permission))
            {
                MenuUrlHelper.TrySplit(_permission, out controllerName, out actionName);
            }
            else if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                controllerName = descriptor.ControllerName;
                actionName = descriptor.ActionName;
            }
            else
            {
                controllerName = string.Empty;
                actionName = string.Empty;
            }

            bool allowed = false;
            long userId = context.HttpContext.GetSessionUserId();
            try
            {
                allowed = await _accessControlService.HasPermissionAsync(userId, controllerName, actionName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while checking permission {Controller}/{Action}", controllerName, actionName);
            }

            if (!allowed)
            {
                _logger.Warning("Access denied: user {UserId} -> {Controller}/{Action}", userId, controllerName, actionName);
                context.Result = DeniedResult(context.HttpContext);
                return; // do NOT call next(): the action must not run
            }

            await next();
        }

        public static IActionResult DeniedResult(HttpContext httpContext)
        {
            if (httpContext.Request.IsAjaxRequest())
            {
                return new JsonResult(new ActionResponse
                {
                    code = StatusCodes.Status403Forbidden,
                    messages = new List<string> { AccessDeniedMessage }
                });
            }
            return new RedirectToActionResult("AccessDenied", "Home", null);
        }
    }
}
