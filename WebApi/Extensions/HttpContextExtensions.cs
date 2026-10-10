using Microsoft.AspNetCore.Mvc;
using Utility;
using Utility.Exceptions;

namespace WebApi.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>True for jQuery/fetch AJAX calls that expect JSON instead of an HTML redirect.</summary>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            var accept = request.Headers.Accept.ToString();
            return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Returns the signed-in ApUser.Id from the session, or 0 when nobody is signed in.</summary>
        public static long GetSessionUserId(this HttpContext? httpContext)
        {
            var value = httpContext?.Session.GetString(SessionKeys.UserId);
            return long.TryParse(value, out var id) && id > 0 ? id : 0;
        }
    }

    public static class SafeErrorMessage
    {
        public const string Generic = "Something went wrong. Please try again or contact the administrator.";

        /// <summary>
        /// Only messages of exceptions we throw on purpose (CustomApplicationException) are shown to the user.
        /// Everything else (SQL errors, null references, ...) is logged but never sent to the browser.
        /// </summary>
        public static List<string> For(Exception ex)
        {
            return ex is CustomApplicationException
                ? new List<string> { ex.Message }
                : new List<string> { Generic };
        }
    }
}
