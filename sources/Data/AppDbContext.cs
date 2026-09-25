using SwiftUI_backend_demo.sources.Models;
using Microsoft.EntityFrameworkCore;

namespace SwiftUI_backend_demo.sources.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Article> Articles { get; set; } = null!;
}