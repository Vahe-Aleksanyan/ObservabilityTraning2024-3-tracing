using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks(); // <-- Add this line


var connectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString));

    
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();

// Map the health check endpoint
app.MapHealthChecks("/health"); // <-- This requires AddHealthChecks()

app.MapControllers();
app.Run("http://*:80");