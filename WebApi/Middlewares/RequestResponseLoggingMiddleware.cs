using System.Diagnostics;
using Utility;

namespace WebApi.Middlewares
{
    /// <summary>
    /// Optional access-log middleware (not registered by default). Register with
    /// app.UseMiddleware&lt;RequestResponseLoggingMiddleware&gt;() after UseForwardedHeaders.
    ///
    /// PCI-DSS: request and response BODIES and query strings are deliberately NOT logged. They can contain
    /// passwords, OTPs, card data or tokens. Only metadata (path, status, timing, client) is written.
    /// Exceptions are not swallowed here; they continue to the normal exception handler.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Serilog.ILogger _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, Serilog.ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var model = new ElasticRequestResponse
            {
                RequestTime = DateTime.UtcNow,
                LocalIPAddress = context.Connection.LocalIpAddress?.ToString() ?? "Unknown",
                LocalPort = context.Connection.LocalPort.ToString(),
                RemoteIPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                RemotePort = context.Connection.RemotePort.ToString(),
                UserAgent = context.Request.Headers.UserAgent.FirstOrDefault(),
                RequestSchema = context.Request.Scheme,
                RequestHost = context.Request.Host.ToString(),
                RequestPath = context.Request.Path
            };
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                model.ResponseTime = DateTime.UtcNow;
                model.StatusCode = context.Response.StatusCode;
                _logger.Information(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms for {RemoteIP} ({UserAgent})",
                    context.Request.Method, model.RequestPath, model.StatusCode, stopwatch.ElapsedMilliseconds,
                    model.RemoteIPAddress, model.UserAgent);
            }
        }
    }
}
