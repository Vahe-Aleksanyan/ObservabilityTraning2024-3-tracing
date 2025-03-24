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
// builder.Services.AddHealthChecks(); // Health checks service

// // Add Prometheus metrics server with options configuration
// builder.Services.AddMetricServer(options =>
// {
//     options.Port = 1234; // Example: Set a specific port for the metrics server
// });

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

// // Add Prometheus HTTP request metrics
// app.UseHttpMetrics();

// app.UseRouting();
// app.UseAuthorization();

// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapMetrics();
// });

// // Map the health check endpoint
// app.MapHealthChecks("/health");

// app.MapControllers();
// app.Run("http://*:80");

using NLogLevel = NLog.LogLevel; 
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

// Clear default providers and configure structured logging with NLog
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

// Configure NLog for structured logging
builder.Host.UseNLog(new NLogAspNetCoreOptions
{
    RemoveLoggerFactoryFilter = false,
    RegisterHttpContextAccessor = true,
    IncludeScopes = true, // Important for structured logging
    ParseMessageTemplates = true // Required for proper structured logging
});

// Configure NLog layout renders for structured logging
LogManager.Setup().LoadConfiguration(builder => {
    builder.ForLogger().FilterMinLevel(NLogLevel.Trace).WriteToConsole(
        layout: "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}${newline}${all-event-properties:format=[key]=[value]}"
    );
    builder.ForLogger().FilterMinLevel(NLogLevel.Trace).WriteToFile(
        fileName: "logs/log-${shortdate}.log",
        layout: "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}${newline}${all-event-properties:format=[key]=[value]}"
    );
});

// Add services to the container.
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks();

// Add Prometheus metrics server with options configuration
builder.Services.AddMetricServer(options =>
{
    options.Port = 1234;
});

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    dbContext.Database.EnsureCreated(); // Creates tables if they don't exist
}


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
    endpoints.MapMetrics();
});

// Map the health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();
app.Run("http://*:80");