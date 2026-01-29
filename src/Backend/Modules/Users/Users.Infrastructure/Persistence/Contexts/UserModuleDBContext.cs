using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Outbox;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Contexts
{
    public class UserModuleDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = default!;
        public UserModuleDbContext(DbContextOptions<UserModuleDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserModuleDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
            
        }
    }
}
