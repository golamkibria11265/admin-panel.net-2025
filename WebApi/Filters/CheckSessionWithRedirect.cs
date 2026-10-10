using Bll.Services.ApService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Utility.Models.ResponseModels;
using WebApi.Extensions;

namespace WebApi.Filters
{
    /// <summary>
    /// Global filter (registered in Program.cs): every action requires a signed-in, active user
    /// unless it is marked with [AllowAnonymous].
    /// </summary>
    public class CheckSessionWithRedirect : IAsyncActionFilter
    {
        private const string SessionExpiredMessage = "Session expired! Please log in again using your credential!";
        private readonly IAccessControlService _accessControlService;
        private readonly Serilog.ILogger _logger;

        public CheckSessionWithRedirect(IAccessControlService accessControlService, Serilog.ILogger logger)
        {
            _accessControlService = accessControlService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            {
                await next();
                return;
            }

            var httpContext = context.HttpContext;
            long userId = httpContext.GetSessionUserId();
            bool isAuthenticated = false;
            try
            {
                isAuthenticated = userId > 0 && await _accessControlService.IsUserActiveAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while validating the session");
            }

            if (isAuthenticated)
            {
                await next();
                return;
            }

            bool hadSession = httpContext.Request.Cookies.ContainsKey(".AdminPanel.Session");
            // Not signed in, session expired, or the user was disabled: drop whatever is left in the session.
            httpContext.Session.Clear();

            if (httpContext.Request.IsAjaxRequest())
            {
                context.Result = new JsonResult(new ActionResponse
                {
                    code = StatusCodes.Status401Unauthorized,
                    session_expired = true,
                    redirect_url = $"{httpContext.Request.PathBase}/User/Login",
                    messages = new List<string> { SessionExpiredMessage }
                });
                return;
            }

            // Show "session expired" only to someone who actually had a session (not to first-time visitors).
            if (hadSession && context.Controller is Controller controller)
            {
                controller.TempData["SessionExpired"] = SessionExpiredMessage;
            }
            context.Result = new RedirectToActionResult("Login", "User", null);
        }
    }
}
