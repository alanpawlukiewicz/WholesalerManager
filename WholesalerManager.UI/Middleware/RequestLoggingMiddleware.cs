using System.Diagnostics;

namespace WholesalerManager.UI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;

            _logger.LogInformation(
                "HTTP {Method} {Path} started | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId
            );

            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} finished | Status: {StatusCode} | Duration: {Duration}ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId
            );
        }
    }
}
