using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketing.Infrastructure.Persistence.Configs;

/// <summary>
/// EF Core configuration for ExternalDistribution child entity.
/// Stored in separate table with foreign key to PostMarketing.
/// </summary>
public class ExternalDistributionConfiguration : IEntityTypeConfiguration<ExternalDistribution>
{
    public void Configure(EntityTypeBuilder<ExternalDistribution> builder)
    {
        // ─────────────────────────────────────────────────────────────
        // Table & Key
        // ─────────────────────────────────────────────────────────────
        builder.ToTable("external_distribution");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        // ─────────────────────────────────────────────────────────────
        // Shadow Foreign Key to Parent Aggregate (PostMarketing)
        // ─────────────────────────────────────────────────────────────
        builder.Property("PostMarketingId")
               .HasColumnName("post_marketing_id")
               .HasColumnType("uuid")
               .IsRequired();

        // ─────────────────────────────────────────────────────────────
        // Properties
        // ─────────────────────────────────────────────────────────────
        
        builder.Property(d => d.Platform)
               .HasColumnName("platform")
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(d => d.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(DistributionStatus.Pending);

        builder.Property(d => d.ExternalUrl)
               .HasColumnName("external_url")
               .HasMaxLength(500);

        builder.Property(d => d.ExternalPostId)
               .HasColumnName("external_post_id")
               .HasMaxLength(255);

        builder.Property(d => d.PlatformMetadata)
               .HasColumnName("platform_metadata")
               .HasColumnType("jsonb");

        builder.Property(d => d.ErrorMessage)
               .HasColumnName("error_message")
               .HasMaxLength(500);

        builder.Property(d => d.SentAt)
               .HasColumnName("sent_at")
               .HasColumnType("timestamp with time zone");

        // ─────────────────────────────────────────────────────────────
        // Indexes
        // ─────────────────────────────────────────────────────────────
        
        builder.HasIndex(d => new { d.Platform, d.Status })
               .HasDatabaseName("ix_external_dist_platform_status");

        builder.HasIndex(d => d.Status)
               .HasDatabaseName("ix_external_dist_status");

        builder.HasIndex(d => EF.Property<Guid>(d, "PostMarketingId"))
               .HasDatabaseName("ix_external_dist_post_id");
    }
}