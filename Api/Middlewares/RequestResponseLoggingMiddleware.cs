using Newtonsoft.Json;
using System.Net;
using Utility;
using Utility.Exceptions;
using Microsoft.IO;

namespace Api.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private const int ReadChunkBufferLength = 4096;
        private ApiErrorResponse? outputResponse = null;
        public IConfiguration _configuration { get; }
        private readonly Serilog.ILogger _logger;
        public RequestResponseLoggingMiddleware(RequestDelegate next, IConfiguration configuration, Serilog.ILogger logger)
        {
            _next = next;
            _configuration = configuration;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            var model = new ElasticRequestResponse()
            {
                RequestTime = DateTime.Now,
                LocalIPAddress = context.Connection.LocalIpAddress?.ToString() ?? "Unknown",
                LocalPort = context.Connection.LocalPort.ToString(),
                RemoteIPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                RemotePort = context.Connection.RemotePort.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
            };
            await FormatRequest(context, model);
            Stream originalBody = context.Response.Body;

            using (MemoryStream newResponseBody = _recyclableMemoryStreamManager.GetStream())
            {
                Stream originalRequestBody = context.Request.Body;
                context.Response.Body = newResponseBody;
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(context, ex);
                }
                finally
                {
                    newResponseBody.Seek(0, SeekOrigin.Begin);
                    await newResponseBody.CopyToAsync(originalBody);

                    newResponseBody.Seek(0, SeekOrigin.Begin);
                    FormatResponse(context, newResponseBody, model);
                    model.ResponseTime = DateTime.Now;
                }
            }
        }

        private async Task FormatRequest(HttpContext context, ElasticRequestResponse model)
        {
            HttpRequest request = context.Request;
            model.RequestSchema = request.Scheme;
            model.RequestHost = request.Host.ToString();
            model.RequestPath = request.Path;
            model.RequestQueryString = request.QueryString.ToString();
            model.RequestBody = await GetRequestBody(request, context);
        }
        private string? FormatResponse(HttpContext context, MemoryStream newResponseBody, ElasticRequestResponse model)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            model.ResponseBody = outputResponse != null ? JsonConvert.SerializeObject(outputResponse) : ReadStreamInChunks(newResponseBody);
            model.StatusCode = response.StatusCode;
            return null;
        }
        public async Task<string> GetRequestBody(HttpRequest request, HttpContext context)
        {

            request.EnableBuffering();
            using (var requestStream = _recyclableMemoryStreamManager.GetStream())
            {
                await request.Body.CopyToAsync(requestStream);
                request.Body.Seek(0, SeekOrigin.Begin);
                return ReadStreamInChunks(requestStream);
            }
        }
        private static string ReadStreamInChunks(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            string result;
            using (var textWriter = new StringWriter())
            using (var reader = new StreamReader(stream))
            {
                var readChunk = new char[ReadChunkBufferLength];
                int readChunkLength;
                do
                {
                    readChunkLength = reader.ReadBlock(readChunk, 0, ReadChunkBufferLength);
                    textWriter.Write(readChunk, 0, readChunkLength);
                } while (readChunkLength > 0);

                result = textWriter.ToString();
            }
            return result;
        }
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            _logger.Error("exception.Message=>" + exception.Message + "");
            _logger.Error("exception.InnerException=>" + exception.InnerException + "");
            _logger.Error($"exception.StackTrace=>{exception.StackTrace}");
            var error = new ApiErrorResponse();
            error.code = (int)code;
            error.messages = new List<string> { exception.Message };
            error.details = exception.StackTrace;
            if (exception is CustomApplicationException)
            {
                error.messages = new List<string> { exception.Message };
                error.details = exception.StackTrace;

                error.code = (int)HttpStatusCode.UnprocessableEntity;
                code = HttpStatusCode.UnprocessableEntity;
            }
            else if (exception is CustomTransactionException)
            {
                error.messages = new List<string> { exception.Message };
                error.details = exception.StackTrace;
                code = HttpStatusCode.UnprocessableEntity;
                var custom = (CustomTransactionException)exception;
                error.code = custom.Code;
            }
            else if (exception is DeviceChangeException)
            {
                error.code = StatusCodes.Status451UnavailableForLegalReasons;
                error.messages = new List<string> { "Device Change, A new OTP send your mobile device" };
                error.details = exception.Message;
                code = HttpStatusCode.UnavailableForLegalReasons;
            }
            else if (exception is UnauthorizedAccessException)
            {
                error.code = StatusCodes.Status401Unauthorized;
                error.messages = new List<string> { "Unauthorized Access" };
                error.details = exception.Message;
                code = HttpStatusCode.Unauthorized;
            }
            else if (exception is UserChangePinPasswordChangeException)
            {
                error.code = 425;
                error.messages = new List<string> { "Your account is Locked" };
                error.details = exception.Message;
                code = HttpStatusCode.UnavailableForLegalReasons;
            }
            var result = JsonConvert.SerializeObject(error);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status200OK;
            return context.Response.WriteAsync(result);
        }
    }
}
