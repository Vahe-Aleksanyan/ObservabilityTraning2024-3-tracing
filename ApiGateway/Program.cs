using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// ===== Tracing Setup =====
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("api-gateway")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "jaeger";
                options.AgentPort = 6831;
            });
    });

builder.Services.AddHttpClient<BackendClient>(client => 
{
    client.BaseAddress = new Uri("http://web/");
});

var app = builder.Build();

app.MapGet("/api/resources", async (BackendClient client) =>
{
    using var activity = Activity.Current?.Source
        .StartActivity("Process.Request");
    
    var response = await client.GetItemsAsync();
    activity?.AddTag("backend.response.code", (int)response.StatusCode);
    
    return Results.Ok(await response.Content.ReadAsStringAsync());
});

app.Run();