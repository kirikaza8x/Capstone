using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AI.Domain.Entities;

namespace AI.Infrastructure.Persistence.Configs
{
    public class UserWeightProfileConfiguration : IEntityTypeConfiguration<UserWeightProfile>
    {
        public void Configure(EntityTypeBuilder<UserWeightProfile> builder)
        {
            builder.ToTable("user_weight_profile");

            // Primary key
            builder.HasKey(uwp => uwp.Id);

            builder.Property(uwp => uwp.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(uwp => uwp.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            builder.Property(uwp => uwp.ActionType)
                   .HasColumnName("action_type")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(uwp => uwp.PersonalizedWeight)
                   .HasColumnName("personalized_weight")
                   .HasColumnType("double precision")
                   .IsRequired();

            builder.Property(uwp => uwp.ConfidenceCount)
                   .HasColumnName("confidence_count")
                   .IsRequired();

            // Auditing
            builder.Property(uwp => uwp.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(uwp => uwp.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(uwp => uwp.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(uwp => uwp.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(uwp => uwp.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Relationships (optional: if you want FK to User)
            // builder.HasOne<User>()
            //        .WithMany()
            //        .HasForeignKey(uwp => uwp.UserId)
            //        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
