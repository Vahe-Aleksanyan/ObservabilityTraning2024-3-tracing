
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
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry setup
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("crud-service")
            .AddAspNetCoreInstrumentation(options => 
            {
                options.RecordException = true;
            })
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
            })
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "jaeger";
                options.AgentPort = 6831;
            });
});

// Configure logging with NLog
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog(new NLogAspNetCoreOptions
{
    RemoveLoggerFactoryFilter = false,
    RegisterHttpContextAccessor = true,
    IncludeScopes = true,
    ParseMessageTemplates = true
});

LogManager.Setup().LoadConfiguration(builder => {
    builder.ForLogger().FilterMinLevel(NLogLevel.Trace).WriteToConsole(
        layout: "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}${newline}${all-event-properties:format=[key]=[value]}"
    );
    builder.ForLogger().FilterMinLevel(NLogLevel.Trace).WriteToFile(
        fileName: "logs/log-${shortdate}.log",
        layout: "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}${newline}${all-event-properties:format=[key]=[value]}"
    );
});

// Add services to the container
builder.Services.AddControllers();
// (Optional) Remove if not using views
builder.Services.AddControllersWithViews();

builder.Services.AddHealthChecks();
builder.Services.AddMetricServer(options => { options.Port = 1234; });

var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapGet("/api/items", async (MyDbContext dbContext) =>
{
    using var activity = Activity.Current?.Source.StartActivity("Query.Items");
    var items = await dbContext.MyModels.ToListAsync();
    activity?.AddTag("db.result.count", items.Count);
    return Results.Ok(items);
});

// Prometheus metrics using top-level mapping
app.UseHttpMetrics();
app.MapMetrics();

// Routing and Authorization
app.UseRouting();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run("http://*:80");
