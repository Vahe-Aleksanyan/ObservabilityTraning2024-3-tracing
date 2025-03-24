// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Logging;
// using Prometheus;

// public class RequestLoggingMiddleware
// {

//     private static readonly Counter RequestCounter = Metrics.CreateCounter("http_requests_total", "Total number of HTTP requests", new CounterConfiguration
//     {
//         LabelNames = new[] { "method", "endpoint", "status_code" }
//     });
//     private readonly RequestDelegate _next;
//     private readonly ILogger<RequestLoggingMiddleware> _logger;

//     public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
//     {
//         _next = next;
//         _logger = logger;
//     }

//     public async Task InvokeAsync(HttpContext httpContext)
//     {

//         var statusCode = httpContext.Response.StatusCode.ToString();
//         RequestCounter.Labels(httpContext.Request.Method, httpContext.Request.Path, statusCode).Inc();

//         _logger.LogInformation("Handling request: {Method} {Url}", httpContext.Request.Method, httpContext.Request.Path);
//         await _next(httpContext);
//         _logger.LogInformation("Finished handling request.");
//     }
// }


using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;

public class RequestLoggingMiddleware
{
    private static readonly Counter RequestCounter = Metrics.CreateCounter(
        "http_requests_total", 
        "Total number of HTTP requests", 
        new CounterConfiguration
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
        // Log request start with structured data
        _logger.LogInformation("Request started: {Method} {Path} {QueryString} {Headers}", 
            httpContext.Request.Method, 
            httpContext.Request.Path,
            httpContext.Request.QueryString,
            httpContext.Request.Headers);

        try
        {
            await _next(httpContext);
            
            // Log successful completion
            _logger.LogInformation("Request completed: {Method} {Path} {StatusCode} {ContentType}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Response.StatusCode,
                httpContext.Response.ContentType);
        }
        catch (Exception ex)
        {
            // Log errors with full context
            _logger.LogError(ex, "Request failed: {Method} {Path} {StatusCode}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Response.StatusCode);
            
            throw; // Re-throw to let the error handling middleware process it
        }
        finally
        {
            // Update metrics
            RequestCounter.Labels(
                httpContext.Request.Method, 
                httpContext.Request.Path, 
                httpContext.Response.StatusCode.ToString()).Inc();
        }
    }
}