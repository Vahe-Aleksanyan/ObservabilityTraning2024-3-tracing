// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.EntityFrameworkCore;
// using MyWebApp.Models;
// using NLog;
// using NLog.Web;
// using Microsoft.Extensions.Logging;
// using Prometheus;



// var builder = WebApplication.CreateBuilder(args);

// builder.Logging.ClearProviders();
// builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
// builder.Host.UseNLog(); 

// // Add services to the container.
// builder.Services.AddControllers();

// // Add health checks
// builder.Services.AddHealthChecks(); // <-- Add this line
// builder.Services.AddMetricServer();
// builder.Services.AddControllersWithViews();

// var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
// builder.Services.AddDbContext<MyDbContext>(options =>
//     options.UseNpgsql(connectionString));


// var app = builder.Build();
// app.UseHttpsRedirection();
// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage();
// }
// app.UseMiddleware<RequestLoggingMiddleware>();

// app.UseRouting();
// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapMetrics();  // Expose Prometheus metrics at /metrics endpoint
// });
// // Map the health check endpoint
// app.MapHealthChecks("/health"); // <-- This requires AddHealthChecks()

// app.MapControllers();
// app.Run("http://*:80");

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using NLog;
using NLog.Web;
using Microsoft.Extensions.Logging;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks(); // Health checks service

// Add Prometheus metrics server with options configuration
builder.Services.AddMetricServer(options =>
{
    options.Port = 1234; // Example: Set a specific port for the metrics server
});

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<RequestLoggingMiddleware>();

// Add Prometheus HTTP request metrics
app.UseHttpMetrics();

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();  // Expose Prometheus metrics at /metrics endpoint
});

// Map the health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();
app.Run("http://*:80");