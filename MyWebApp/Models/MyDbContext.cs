using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Models/MyDbContext.cs

namespace MyWebApp.Models
{
    public class MyDbContext : DbContext
    {
        public DbSet<MyModel> MyModels { get; set; }
        private readonly ILogger<MyDbContext> _logger;
        public MyDbContext(DbContextOptions<MyDbContext> options, ILogger<MyDbContext> logger) : base(options) { 
             _logger = logger;
        }

         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        
        optionsBuilder.LogTo(query => _logger.LogInformation("Executing SQL: {Query}", query));
    }
    }
}