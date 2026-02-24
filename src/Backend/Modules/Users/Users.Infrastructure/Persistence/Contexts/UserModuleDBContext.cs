using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Contexts
{
    public class UserModuleDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!; 

        public UserModuleDbContext(DbContextOptions<UserModuleDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserModuleDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
