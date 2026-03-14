using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AI.Domain.Entities;
using Pgvector;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AI.Infrastructure.Persistence.Configs
{
    public class UserEmbeddingConfiguration : IEntityTypeConfiguration<UserEmbedding>
    {
        public void Configure(EntityTypeBuilder<UserEmbedding> builder)
        {
            builder.ToTable("user_embedding");

            // Primary key
            builder.HasKey(ue => ue.Id);

            builder.Property(ue => ue.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(ue => ue.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            // Vector column using pgvector with value converter
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

            builder.Property(ue => ue.Embedding)
                   .HasColumnName("embedding")
                   .HasColumnType("vector(384)")
                   .HasConversion(vectorConverter)
                   .IsRequired();
            builder.Property(ue => ue.Embedding).Metadata.SetValueComparer(vectorComparer);

            builder.Property(ue => ue.Dimension)
                   .HasColumnName("dimension")
                   .IsRequired();

            builder.Property(ue => ue.InteractionCount)
                   .HasColumnName("interaction_count")
                   .IsRequired();

            builder.Property(ue => ue.Confidence)
                   .HasColumnName("confidence")
                   .HasColumnType("double precision")
                   .IsRequired();

            builder.Property(ue => ue.LastCalculated)
                   .HasColumnName("last_calculated")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(ue => ue.IsStale)
                   .HasColumnName("is_stale")
                   .HasDefaultValue(false)
                   .IsRequired();

            // Auditing
            builder.Property(ue => ue.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(ue => ue.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(ue => ue.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(ue => ue.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(ue => ue.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Index for user lookup
            builder.HasIndex(ue => ue.UserId)
                   .HasDatabaseName("ix_user_embedding_user_id");

            // Index for stale detection (background job optimization)
            builder.HasIndex(ue => ue.IsStale)
                   .HasDatabaseName("ix_user_embedding_is_stale");

            // HNSW index for vector similarity search (cosine distance)
            // HNSW is recommended for datasets < 1M vectors with high accuracy requirements
            builder.HasIndex(ue => ue.Embedding)
                   .HasMethod("hnsw")
                   .HasOperators("vector_cosine_ops")
                   .HasStorageParameter("m", 16)
                   .HasStorageParameter("ef_construction", 64);

            var jsonOptions = new JsonSerializerOptions();
            var jsonConverter = new ValueConverter<IReadOnlyCollection<string>, string>(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions)!
            );

            var jsonComparer = new ValueComparer<IReadOnlyCollection<string>>(
                (c1, c2) => ReferenceEquals(c1, c2) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? Array.Empty<string>() : c.ToList()
            );

            builder.Property(ue => ue.ContributingCategories)
                   .HasColumnName("contributing_categories")
                   .HasColumnType("jsonb")
                   .HasConversion(jsonConverter);
            builder.Property(ue => ue.ContributingCategories).Metadata.SetValueComparer(jsonComparer);

            // IVFFlat index for vector similarity search (created via migration)
            // Note: Vector indexes must be created after the table exists
            // builder.HasIndex(ue => ue.Embedding)
            //        .HasMethod("ivfflat")
            //        .HasOperators("vector_cosine_ops");
        }
    }
}
