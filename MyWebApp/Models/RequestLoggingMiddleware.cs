using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;

public class RequestLoggingMiddleware
{

    private static readonly Counter RequestCounter = Metrics.CreateCounter("http_requests_total", "Total number of HTTP requests", new CounterConfiguration
    {
        LabelNames = new[] { "method", "endpoint", "status_code" }
    });
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {

        var statusCode = httpContext.Response.StatusCode.ToString();
        RequestCounter.Labels(httpContext.Request.Method, httpContext.Request.Path, statusCode).Inc();

        _logger.LogInformation("Handling request: {Method} {Url}", httpContext.Request.Method, httpContext.Request.Path);
        await _next(httpContext);
        _logger.LogInformation("Finished handling request.");
    }
}
