// Models/MyDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace MyWebApp.Models
{
    public class MyDbContext : DbContext
    {
        public DbSet<MyModel> MyModels { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    }
}