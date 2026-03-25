using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Extensions;

namespace Marketing.Infrastructure.Persistence.Configs;

public class PostConfiguration : IEntityTypeConfiguration<PostMarketing>
{
    public void Configure(EntityTypeBuilder<PostMarketing> builder)
    {
        // ─────────────────────────────────────────────────────────────
        // Table & Key
        // ─────────────────────────────────────────────────────────────
        builder.ToTable("post");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        // ─────────────────────────────────────────────────────────────
        // Identity & Ownership
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.EventId)
               .HasColumnName("event_id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(p => p.OrganizerId)
               .HasColumnName("organizer_id")
               .HasColumnType("uuid")
               .IsRequired();

        // ─────────────────────────────────────────────────────────────
        // Content
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.Title)
               .HasColumnName("title")
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(p => p.Body)
               .HasColumnName("body")
               .IsRequired()
               .HasMaxLength(4000);

        builder.Property(p => p.ImageUrl)
               .HasColumnName("image_url")
               .HasMaxLength(500);

        // ─────────────────────────────────────────────────────────────
        // AI Metadata
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.PromptUsed)
               .HasColumnName("prompt_used")
               .HasMaxLength(2000);

        builder.Property(p => p.AiModel)
               .HasColumnName("ai_model")
               .HasMaxLength(100);

        builder.Property(p => p.AiTokensUsed)
               .HasColumnName("ai_tokens_used");

        // ─────────────────────────────────────────────────────────────
        // Moderation
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(PostStatus.Draft);

        builder.Property(p => p.ReviewedBy)
               .HasColumnName("reviewed_by")
               .HasColumnType("uuid");

        builder.Property(p => p.ReviewedAt)
               .HasColumnName("reviewed_at")
               .HasColumnType("timestamp with time zone");

        builder.Property(p => p.RejectionReason)
               .HasColumnName("rejection_reason")
               .HasMaxLength(500);

        // ─────────────────────────────────────────────────────────────
        // Publishing
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.PublishedAt)
               .HasColumnName("published_at")
               .HasColumnType("timestamp with time zone");

        builder.Property(p => p.SubmittedAt)
               .HasColumnName("submitted_at")
               .HasColumnType("timestamp with time zone");

        builder.Property(p => p.TrackingToken)
               .HasColumnName("tracking_token")
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(p => p.ExternalPostUrl)
               .HasColumnName("external_post_url")
               .HasMaxLength(500);

        // ─────────────────────────────────────────────────────────────
        // Versioning
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.Version)
               .HasColumnName("version")
               .IsRequired()
               .HasDefaultValue(1);

        // ─────────────────────────────────────────────────────────────
        // Audit Fields (CreatedAt, ModifiedAt from AggregateRoot)
        // ─────────────────────────────────────────────────────────────
        builder.ConfigureAudit<PostMarketing, Guid>();

        // ─────────────────────────────────────────────────────────────
        // Indexes
        // ─────────────────────────────────────────────────────────────
        
        // Unique index on TrackingToken (attribution)
        builder.HasIndex(p => p.TrackingToken)
               .IsUnique()
               .HasDatabaseName("ix_post_tracking_token");

        // Composite index for organizer queries (dashboard filtering)
        builder.HasIndex(p => new { p.OrganizerId, p.EventId, p.Status })
               .HasDatabaseName("ix_post_organizer_event_status");

        // Index for pending moderation queue (FIFO ordering)
        builder.HasIndex(p => new { p.Status, p.SubmittedAt })
               .HasDatabaseName("ix_post_pending_queue")
               .HasFilter("status = 'Pending'");

        // Index for published posts by event (public feed)
        builder.HasIndex(p => new { p.EventId, p.Status, p.PublishedAt })
               .HasDatabaseName("ix_post_published_by_event")
               .HasFilter("status = 'Published'");
    }
}