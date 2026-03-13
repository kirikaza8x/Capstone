using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AI.Domain.Entities;

namespace AI.Infrastructure.Persistence.Configs
{
    public class CategoryCoOccurrenceConfiguration : IEntityTypeConfiguration<CategoryCoOccurrence>
    {
        public void Configure(EntityTypeBuilder<CategoryCoOccurrence> builder)
        {
            builder.ToTable("category_co_occurrence");

            // Primary key
            builder.HasKey(cco => cco.Id);

            builder.Property(cco => cco.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(cco => cco.Category1)
                   .HasColumnName("category1")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(cco => cco.Category2)
                   .HasColumnName("category2")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(cco => cco.CoOccurrenceCount)
                   .HasColumnName("co_occurrence_count")
                   .HasDefaultValue(1)
                   .IsRequired();

            builder.Property(cco => cco.LiftScore)
                   .HasColumnName("lift_score")
                   .HasColumnType("double precision")
                   .HasDefaultValue(1.0)
                   .IsRequired();

            builder.Property(cco => cco.ConfidenceAtoB)
                   .HasColumnName("confidence_a_to_b")
                   .HasColumnType("double precision")
                   .HasDefaultValue(1.0)
                   .IsRequired();

            builder.Property(cco => cco.ConfidenceBtoA)
                   .HasColumnName("confidence_b_to_a")
                   .HasColumnType("double precision")
                   .HasDefaultValue(1.0)
                   .IsRequired();

            builder.Property(cco => cco.LastUpdated)
                   .HasColumnName("last_updated")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(cco => cco.Category1TotalCount)
                   .HasColumnName("category1_total_count")
                   .HasDefaultValue(1)
                   .IsRequired();

            builder.Property(cco => cco.Category2TotalCount)
                   .HasColumnName("category2_total_count")
                   .HasDefaultValue(1)
                   .IsRequired();

            builder.Property(cco => cco.TotalSessions)
                   .HasColumnName("total_sessions")
                   .HasDefaultValue(1)
                   .IsRequired();

            // Auditing
            builder.Property(cco => cco.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(cco => cco.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(cco => cco.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(cco => cco.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(cco => cco.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Unique index on category pair (enforced ordering: category1 < category2)
            builder.HasIndex(cco => new { cco.Category1, cco.Category2 })
                   .IsUnique()
                   .HasDatabaseName("ux_category_co_occurrence_category1_category2");

            // Indexes for category lookups (for "users who liked X also liked Y")
            builder.HasIndex(cco => cco.Category1)
                   .HasDatabaseName("ix_category_co_occurrence_category1");

            builder.HasIndex(cco => cco.Category2)
                   .HasDatabaseName("ix_category_co_occurrence_category2");

            // Composite index for efficient bidirectional lookup
            var index1 = builder.HasIndex(cco => cco.Category1)
                .HasDatabaseName("ix_category_co_occurrence_category1_lookup");

            NpgsqlIndexBuilderExtensions.IncludeProperties(index1,
                nameof(CategoryCoOccurrence.Category2),
                nameof(CategoryCoOccurrence.LiftScore),
                nameof(CategoryCoOccurrence.CoOccurrenceCount));

            var index2 = builder.HasIndex(cco => cco.Category2)
                .HasDatabaseName("ix_category_co_occurrence_category2_lookup");

            NpgsqlIndexBuilderExtensions.IncludeProperties(index2,
                nameof(CategoryCoOccurrence.Category1),
                nameof(CategoryCoOccurrence.LiftScore),
                nameof(CategoryCoOccurrence.CoOccurrenceCount));
        }
    }
}
