using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AI.Domain.Entities;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AI.Infrastructure.Persistence.Configs
{
    /// <summary>
    /// EF Core configuration for EventSnapshot entity.
    /// Maps to PostgreSQL with JSONB for collections.
    /// </summary>
    public class EventSnapshotConfiguration : IEntityTypeConfiguration<EventSnapshot>
    {
        public void Configure(EntityTypeBuilder<EventSnapshot> builder)
        {
            builder.ToTable("event_snapshots");

            // ─────────────────────────────────────────────────────────────
            // Primary Key: Uses external Event ID (not auto-generated)
            // ─────────────────────────────────────────────────────────────
            builder.HasKey(es => es.Id);

            builder.Property(es => es.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .ValueGeneratedNever();

            // ─────────────────────────────────────────────────────────────
            // Core Properties
            // ─────────────────────────────────────────────────────────────
            builder.Property(es => es.Title)
                   .HasColumnName("title")
                   .HasColumnType("varchar(500)")
                   .IsRequired();

            builder.Property(es => es.Description)
                   .HasColumnName("description")
                   .HasColumnType("text")
                   .IsRequired(false);

            builder.Property(es => es.SnapshotUpdatedAt)
                   .HasColumnName("snapshot_updated_at")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            // ─────────────────────────────────────────────────────────────
            // Collections as JSONB (categories, hashtags)
            // ─────────────────────────────────────────────────────────────
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var stringListConverter = new ValueConverter<IReadOnlyCollection<string>, string>(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            );

            var stringListComparer = new ValueComparer<IReadOnlyCollection<string>>(
                (c1, c2) => ReferenceEquals(c1, c2) || 
                           (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<string>() : c.ToList()
            );

            builder.Property(es => es.Categories)
                   .HasColumnName("categories")
                   .HasColumnType("jsonb")
                   .HasConversion(stringListConverter);
            builder.Property(es => es.Categories).Metadata.SetValueComparer(stringListComparer);

            builder.Property(es => es.Hashtags)
                   .HasColumnName("hashtags")
                   .HasColumnType("jsonb")
                   .HasConversion(stringListConverter);
            builder.Property(es => es.Hashtags).Metadata.SetValueComparer(stringListComparer);

            // ─────────────────────────────────────────────────────────────
            // Auditing & Lifecycle Fields
            // ─────────────────────────────────────────────────────────────
            builder.Property(es => es.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(es => es.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true)
                   .IsRequired();

            // ─────────────────────────────────────────────────────────────
            // Indexes
            // ─────────────────────────────────────────────────────────────
            builder.HasIndex(es => es.IsActive)
                   .HasDatabaseName("ix_event_snapshots_is_active");

            builder.HasIndex(es => es.SnapshotUpdatedAt)
                   .HasDatabaseName("ix_event_snapshots_updated_at");

            builder.HasIndex(es => es.Title)
                   .HasDatabaseName("ix_event_snapshots_title");

            builder.HasIndex(es => es.Categories)
                   .HasMethod("gin")
                   .HasOperators("jsonb_path_ops")
                   .HasDatabaseName("ix_event_snapshots_categories_gin");

            builder.HasIndex(es => es.Hashtags)
                   .HasMethod("gin")
                   .HasOperators("jsonb_path_ops")
                   .HasDatabaseName("ix_event_snapshots_hashtags_gin");

            builder.HasIndex(es => new { es.IsActive, es.SnapshotUpdatedAt })
                   .HasDatabaseName("ix_event_snapshots_active_updated");
        }
    }
}