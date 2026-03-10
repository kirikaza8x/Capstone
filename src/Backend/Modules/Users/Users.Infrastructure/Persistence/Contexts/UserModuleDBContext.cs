using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Outbox;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Contexts;

public class UserModuleDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public UserModuleDbContext(DbContextOptions<UserModuleDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Constants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserModuleDbContext).Assembly);
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
