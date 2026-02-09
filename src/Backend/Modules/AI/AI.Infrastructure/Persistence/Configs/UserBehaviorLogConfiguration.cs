using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AI.Domain.Entities;

namespace AI.Infrastructure.Persistence.Configs
{
    public class UserBehaviorLogConfiguration : IEntityTypeConfiguration<UserBehaviorLog>
    {
        public void Configure(EntityTypeBuilder<UserBehaviorLog> builder)
        {
            builder.ToTable("user_behavior_log");

            // Primary key
            builder.HasKey(ubl => ubl.Id);

            builder.Property(ubl => ubl.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Identity & Context
            builder.Property(ubl => ubl.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            // The "What"
            builder.Property(ubl => ubl.ActionType)
                   .HasColumnName("action_type")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(ubl => ubl.TargetId)
                   .HasColumnName("target_id")
                   .HasMaxLength(256)
                   .IsRequired();

            builder.Property(ubl => ubl.TargetType)
                   .HasColumnName("target_type")
                   .HasMaxLength(100)
                   .IsRequired();

            // The "When"
            builder.Property(ubl => ubl.OccurredAt)
                   .HasColumnName("occurred_at")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            // Flexible Data (JSON)
            builder.Property<Dictionary<string, string>>("_metadata")
                   .HasColumnName("metadata")
                   .HasColumnType("jsonb");

            // Auditing
            builder.Property(ubl => ubl.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(ubl => ubl.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(ubl => ubl.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(ubl => ubl.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(ubl => ubl.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Relationships (optional: if you want FK to User)
            // builder.HasOne<User>()
            //        .WithMany()
            //        .HasForeignKey(ubl => ubl.UserId)
            //        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
