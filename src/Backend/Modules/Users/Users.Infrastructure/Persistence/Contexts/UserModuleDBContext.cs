using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Contexts
{
    public class UserModuleDbContext : DbContextBase
    {
        public DbSet<User> Users { get; set; } = default!;
        public UserModuleDbContext(DbContextOptions<UserModuleDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T> in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserModuleDbContext).Assembly);
        }
    }
}
