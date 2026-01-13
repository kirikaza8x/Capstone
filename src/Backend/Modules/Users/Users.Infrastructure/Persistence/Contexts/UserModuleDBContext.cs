using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Outbox;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Contexts
{
    public class UserModuleDbContext : DbContextBase
    {
        public DbSet<User> Users { get; set; } = default!;
        public UserModuleDbContext(DbContextOptions<UserModuleDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserModuleDbContext).Assembly);
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            base.OnModelCreating(modelBuilder);
            
        }
    }
}
