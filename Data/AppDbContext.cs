using Microsoft.EntityFrameworkCore;
using OrderManagementService.Domain;

namespace OrderManagementService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<RefreshToken> RefreshTokens { get; set; }
}