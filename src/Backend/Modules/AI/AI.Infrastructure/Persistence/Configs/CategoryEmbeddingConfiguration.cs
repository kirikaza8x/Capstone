using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AI.Domain.Entities;
using Pgvector;

namespace AI.Infrastructure.Persistence.Configs
{
    public class CategoryEmbeddingConfiguration : IEntityTypeConfiguration<CategoryEmbedding>
    {
        public void Configure(EntityTypeBuilder<CategoryEmbedding> builder)
        {
            builder.ToTable("category_embedding");

            // Primary key
            builder.HasKey(ce => ce.Id);

            builder.Property(ce => ce.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(ce => ce.Category)
                   .HasColumnName("category")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(ce => ce.Description)
                   .HasColumnName("description")
                   .HasMaxLength(1000);

            // Vector column using pgvector with value converter and comparer
            // Domain uses float[], database uses Vector type
            var vectorConverter = new ValueConverter<float[], Vector>(
                v => new Vector(v),
                v => v.ToArray()
            );

            var vectorComparer = new ValueComparer<float[]>(
                (c1, c2) => ReferenceEquals(c1, c2) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? Array.Empty<float>() : c.ToArray()
            );

            builder.Property(ce => ce.Embedding)
                   .HasColumnName("embedding")
                   .HasColumnType("vector(384)")
                   .HasConversion(vectorConverter)
                   .IsRequired();
            builder.Property(ce => ce.Embedding).Metadata.SetValueComparer(vectorComparer);

            builder.Property(ce => ce.Dimension)
                   .HasColumnName("dimension")
                   .IsRequired();

            builder.Property(ce => ce.ModelName)
                   .HasColumnName("model_name")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(ce => ce.LastUpdated)
                   .HasColumnName("last_updated")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(ce => ce.RecommendationCount)
                   .HasColumnName("recommendation_count")
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.Property(ce => ce.ClickThroughCount)
                   .HasColumnName("click_through_count")
                   .HasDefaultValue(0)
                   .IsRequired();

            // Auditing
            builder.Property(ce => ce.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(ce => ce.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(ce => ce.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(ce => ce.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(ce => ce.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Unique index on category name
            builder.HasIndex(ce => ce.Category)
                   .IsUnique()
                   .HasDatabaseName("ux_category_embedding_category");

            // Index for model lookup (useful during model migrations)
            builder.HasIndex(ce => ce.ModelName)
                   .HasDatabaseName("ix_category_embedding_model_name");

            // HNSW index for vector similarity search (cosine distance)
            // HNSW is recommended for datasets < 1M vectors with high accuracy requirements
            builder.HasIndex(ce => ce.Embedding)
                   .HasMethod("hnsw")
                   .HasOperators("vector_cosine_ops")
                   .HasStorageParameter("m", 16)
                   .HasStorageParameter("ef_construction", 64);
        }
    }
}
