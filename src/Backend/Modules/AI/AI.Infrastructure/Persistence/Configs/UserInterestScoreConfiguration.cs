using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AI.Domain.Entities;

namespace AI.Infrastructure.Persistence.Configs
{
    public class UserInterestScoreConfiguration : IEntityTypeConfiguration<UserInterestScore>
    {
        public void Configure(EntityTypeBuilder<UserInterestScore> builder)
        {
            builder.ToTable("user_interest_score");

            // Primary key
            builder.HasKey(uis => uis.Id);

            builder.Property(uis => uis.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(uis => uis.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            builder.Property(uis => uis.Category)
                   .HasColumnName("category")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(uis => uis.Score)
                   .HasColumnName("score")
                   .HasColumnType("double precision")
                   .IsRequired();

            builder.Property(uis => uis.TotalInteractions)
                   .HasColumnName("total_interactions")
                   .IsRequired();

            builder.Property(uis => uis.LastUpdated)
                   .HasColumnName("last_updated")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            // Auditing
            builder.Property(uis => uis.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(uis => uis.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(uis => uis.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(uis => uis.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(uis => uis.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Relationships (optional: if you want FK to User)
            // builder.HasOne<User>()
            //        .WithMany()
            //        .HasForeignKey(uis => uis.UserId)
            //        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
