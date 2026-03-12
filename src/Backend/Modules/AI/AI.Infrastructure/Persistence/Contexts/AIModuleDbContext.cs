using Microsoft.EntityFrameworkCore;
using AI.Domain.Entities;
using AI.Domain.ReadModels;
using Shared.Infrastructure.Outbox;

namespace AI.Infrastructure.Data
{
    public class AIModuleDbContext : DbContext
    {
        public AIModuleDbContext(DbContextOptions<AIModuleDbContext> options) : base(options) { }

        public DbSet<UserBehaviorLog> UserBehaviorLogs { get; set; }
        public DbSet<UserInterestScore> UserInterestScores { get; set; }
        public DbSet<GlobalCategoryStat> GlobalCategoryStats { get; set; }
        public DbSet<InteractionWeight> InteractionWeights { get; set; }
        public DbSet<UserWeightProfile> UserWeightProfiles { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Constants.SchemaName);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AIModuleDbContext).Assembly);
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

            base.OnModelCreating(modelBuilder);
            // Apply configurations if you have separate configuration files
            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(AIDbContext).Assembly);

            // Or simple fluent API here:
            // modelBuilder.Entity<UserInterestScore>()
            //     .HasIndex(x => new { x.UserId, x.Category }) // Speed up lookups!
            //     .IsUnique();
        }
    }
}