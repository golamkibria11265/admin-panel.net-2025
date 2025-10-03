using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebApi.Filters
{
    public class CheckSessionWithRedirect : Attribute, IActionFilter
    {
        private readonly IHttpContextAccessor _iHttpContextAccessor;
        private readonly Serilog.ILogger _logger;
        public CheckSessionWithRedirect(IHttpContextAccessor iHttpContextAccessor, Serilog.ILogger logger)
        {
            _iHttpContextAccessor = iHttpContextAccessor;
            _logger = logger;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                Controller? controller = context.Controller as Controller;
                if (controller == null)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Login",
                        controller = "User"
                    }));
                    return;
                }
                if (context.Exception != null)
                {
                    context.ExceptionHandled = true;
                    controller.TempData["SessionExpired"] = context.Exception.Message.ToString();
                    context.Result = new RedirectToRouteResult
                       (
                       new RouteValueDictionary(new
                       {
                           action = "Login",
                           controller = "User"
                       }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    action = "Login",
                    controller = "User"
                }));
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var sessionInfo = _iHttpContextAccessor.HttpContext?.Session.Keys.ToList();
                Controller? controller = context.Controller as Controller;
                if (controller == null)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Login",
                        controller = "User"
                    }));
                    return;
                }
                if (sessionInfo == null)
                {
                    controller.TempData["SessionExpired"] = "Session Expired! Please log in again using your credential!";
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Login",
                        controller = "User"
                    }));
                    return;
                }
                if (sessionInfo.Count == 0)
                {
                    controller.TempData["SessionExpired"] = "Session Expired! Please log in again using your credential!";
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Login",
                        controller = "User"
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    action = "Login",
                    controller = "User"
                }));
            }
        }
    }
}
