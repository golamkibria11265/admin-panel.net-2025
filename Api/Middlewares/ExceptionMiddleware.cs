using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Exceptions;

namespace Api.Middlewares
{
    public static class ExceptionMiddleware
    {
        public static void builtinExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error => {
                error.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>();
                    var contextHttp = context.Features.Get<HttpRequestFeature>();
                    var code = HttpStatusCode.InternalServerError;
                    var error = new ApiErrorResponse();
                    error.code = (int)code;
                    string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (exception != null)
                    {
                        if (environment == "Development")
                        {
                            error.messages = new List<string> { exception.Error.Message };
                            error.details = exception.Error.StackTrace;
                        }
                        else
                        {
                            error.messages = new List<string> { "A server error occured" };
                            error.details = exception.Error.Message;
                        }
                    }
                    if (exception is CustomApplicationException)
                    {
                        error.code = (int)HttpStatusCode.UnprocessableEntity;
                        code = HttpStatusCode.UnprocessableEntity;
                    }
                    else if (exception is CustomTransactionException)
                    {
                        code = HttpStatusCode.UnprocessableEntity;
                        var custom = (CustomTransactionException)exception;
                        error.code = custom.Code;
                    }
                    else if (exception is DeviceChangeException)
                    {
                        error.code = StatusCodes.Status451UnavailableForLegalReasons;
                        error.messages = new List<string> { "Device Change, A new OTP send your mobile device" };
                        error.details = exception.Error.Message;
                        code = HttpStatusCode.UnavailableForLegalReasons;
                    }
                    else if (exception is UnauthorizedAccessException)
                    {
                        error.code = StatusCodes.Status401Unauthorized;
                        error.messages = new List<string> { "Unauthorized Access" };
                        error.details = exception.Error.Message;
                        code = HttpStatusCode.Unauthorized;
                    }
                    var result = JsonConvert.SerializeObject(error);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)code;
                    await context.Response.WriteAsync(result);
                });
            });
        }
    }
}
