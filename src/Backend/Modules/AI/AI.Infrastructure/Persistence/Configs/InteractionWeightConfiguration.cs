using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AI.Domain.Entities;

namespace AI.Infrastructure.Persistence.Configs
{
    public class InteractionWeightConfiguration : IEntityTypeConfiguration<InteractionWeight>
    {
        public void Configure(EntityTypeBuilder<InteractionWeight> builder)
        {
            builder.ToTable("interaction_weight");

            // Primary key
            builder.HasKey(iw => iw.Id);

            builder.Property(iw => iw.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(iw => iw.ActionType)
                   .HasColumnName("action_type")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(iw => iw.Weight)
                   .HasColumnName("weight")
                   .HasColumnType("double precision")
                   .IsRequired();

            builder.Property(iw => iw.Description)
                   .HasColumnName("description")
                   .HasMaxLength(512);

            // Auditing
            builder.Property(iw => iw.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(iw => iw.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(iw => iw.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(iw => iw.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(iw => iw.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);
        }
    }
}
