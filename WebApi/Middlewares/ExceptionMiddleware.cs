using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using Utility;
using Utility.Exceptions;

namespace WebApi.Middlewares
{
    /// <summary>
    /// Optional JSON exception handler for API-style endpoints (not registered by default; Program.cs uses
    /// /Home/Error for the MVC pages). Register with app.builtinExceptionHandler() if needed.
    /// </summary>
    public static class ExceptionMiddleware
    {
        public static void builtinExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error => {
                error.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();
                    Exception? exception = feature?.Error; // check the exception itself, not the feature object
                    var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                    var code = HttpStatusCode.InternalServerError;
                    var error = new ApiErrorResponse
                    {
                        code = (int)code,
                        messages = new List<string> { "A server error occured" }
                    };
                    if (exception != null && environment.IsDevelopment())
                    {
                        // Details only in Development; never send internal messages or stack traces in production.
                        error.messages = new List<string> { exception.Message };
                        error.details = exception.StackTrace;
                    }
                    switch (exception)
                    {
                        case CustomTransactionException custom:
                            code = HttpStatusCode.UnprocessableEntity;
                            error.code = custom.Code;
                            error.messages = new List<string> { custom.Message };
                            break;
                        case CustomApplicationException custom:
                            code = HttpStatusCode.UnprocessableEntity;
                            error.code = (int)code;
                            error.messages = new List<string> { custom.Message };
                            break;
                        case DeviceChangeException:
                            code = HttpStatusCode.UnavailableForLegalReasons;
                            error.code = StatusCodes.Status451UnavailableForLegalReasons;
                            error.messages = new List<string> { "Device Change, A new OTP send your mobile device" };
                            break;
                        case UnauthorizedAccessException:
                            code = HttpStatusCode.Unauthorized;
                            error.code = StatusCodes.Status401Unauthorized;
                            error.messages = new List<string> { "Unauthorized Access" };
                            break;
                    }
                    if (exception != null)
                    {
                        var logger = context.RequestServices.GetRequiredService<Serilog.ILogger>();
                        logger.Error(exception, "Unhandled exception for {Path}", context.Request.Path);
                    }
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)code;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                });
            });
        }
    }
}
