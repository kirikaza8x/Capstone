using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AI.Domain.ReadModels;

namespace AI.Infrastructure.Persistence.Configs
{
    public class GlobalCategoryStatConfiguration : IEntityTypeConfiguration<GlobalCategoryStat>
    {
        public void Configure(EntityTypeBuilder<GlobalCategoryStat> builder)
        {
            builder.ToTable("global_category_stat");

            // Primary key
            builder.HasKey(gcs => gcs.Id);

            builder.Property(gcs => gcs.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(gcs => gcs.Category)
                   .HasColumnName("category")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(gcs => gcs.PopularityScore)
                   .HasColumnName("popularity_score")
                   .HasColumnType("double precision")
                   .IsRequired();

            builder.Property(gcs => gcs.TotalInteractions)
                   .HasColumnName("total_interactions")
                   .IsRequired();

            builder.Property(gcs => gcs.LastCalculated)
                   .HasColumnName("last_calculated")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            // Auditing
            builder.Property(gcs => gcs.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(gcs => gcs.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(gcs => gcs.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(gcs => gcs.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(gcs => gcs.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);
        }
    }
}
