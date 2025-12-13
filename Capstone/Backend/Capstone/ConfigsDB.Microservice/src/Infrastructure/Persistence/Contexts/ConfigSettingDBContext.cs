using ConfigsDB.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConfigsDB.Infrastructure.Persistence.Contexts
{
    public class ConfigSettingDbContext : DbContext
    {
        public DbSet<ConfigSetting> ConfigSettings { get; set; } = default!;

        public ConfigSettingDbContext(DbContextOptions<ConfigSettingDbContext> options) : base(options) {}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T> in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigSettingDbContext).Assembly);
        }
    }
}
