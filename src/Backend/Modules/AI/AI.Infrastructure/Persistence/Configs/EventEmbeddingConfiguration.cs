// using AI.Domain.Entities;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.ChangeTracking;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
// using Pgvector;

// namespace AI.Infrastructure.Persistence.Configs
// {
//     public class EventEmbeddingConfiguration : IEntityTypeConfiguration<EventEmbedding>
//     {
//         public void Configure(EntityTypeBuilder<EventEmbedding> builder)
//         {
//             builder.ToTable("event_embeddings");

//             builder.HasKey(e => e.Id);

//             builder.Property(e => e.Id)
//                    .HasColumnName("id")
//                    .HasColumnType("uuid")
//                    .HasDefaultValueSql("gen_random_uuid()");

//             builder.Property(e => e.EventId)
//                    .HasColumnName("event_id")
//                    .HasColumnType("uuid")
//                    .IsRequired();

//             builder.HasIndex(e => e.EventId)
//                    .IsUnique()
//                    .HasDatabaseName("ix_event_embeddings_event_id");

//             var vectorConverter = new ValueConverter<float[], Vector>(
//                 v => new Vector(v),
//                 v => v.ToArray()
//             );
//             var vectorComparer = new ValueComparer<float[]>(
//                 (c1, c2) => ReferenceEquals(c1, c2) ||
//                             (c1 != null && c2 != null && c1.SequenceEqual(c2)),
//                 c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
//                 c => c == null ? Array.Empty<float>() : c.ToArray()
//             );

//             builder.Property(e => e.Embedding)
//                    .HasColumnName("embedding")
//                    .HasColumnType("vector(384)")
//                    .HasConversion(vectorConverter)
//                    .IsRequired();
//             builder.Property(e => e.Embedding).Metadata.SetValueComparer(vectorComparer);

//             builder.Property(e => e.ModelName)
//                    .HasColumnName("model_name")
//                    .HasMaxLength(100)
//                    .IsRequired();

//             builder.Property(e => e.EmbeddedAt)
//                    .HasColumnName("embedded_at")
//                    .HasColumnType("timestamp with time zone")
//                    .IsRequired();

//             // Auditing
//             builder.Property(e => e.CreatedAt)
//                    .HasColumnName("created_at")
//                    .HasColumnType("timestamp with time zone")
//                    .HasDefaultValueSql("NOW()");

//             builder.Property(e => e.ModifiedAt)
//                    .HasColumnName("modified_at")
//                    .HasColumnType("timestamp with time zone");

//             builder.Property(e => e.IsActive)
//                    .HasColumnName("is_active")
//                    .HasDefaultValue(true);

//             // HNSW index — same as your UserEmbedding
//             builder.HasIndex(e => e.Embedding)
//                    .HasMethod("hnsw")
//                    .HasOperators("vector_cosine_ops")
//                    .HasStorageParameter("m", 16)
//                    .HasStorageParameter("ef_construction", 64);
//         }
//     }
// }