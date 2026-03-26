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
               .HasColumnType("text"); // better for large AI content

        builder.Property(p => p.Summary)
               .HasColumnName("summary")
               .HasMaxLength(500);

        builder.Property(p => p.Slug)
               .HasColumnName("slug")
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(p => p.ImageUrl)
               .HasColumnName("image_url")
               .HasMaxLength(500);

        // Tags → JSONB (PostgreSQL optimized)
       //  builder.Property(p => p.Tags)
       //         .HasColumnName("tags")
       //         .HasColumnType("jsonb");

        // ─────────────────────────────────────────────────────────────
        // AI Metadata
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.PromptUsed)
               .HasColumnName("prompt_used")
               .HasColumnType("text");

        builder.Property(p => p.AiModel)
               .HasColumnName("ai_model")
               .HasMaxLength(100);

        builder.Property(p => p.AiTokensUsed)
               .HasColumnName("ai_tokens_used");

        builder.Property(p => p.AiCost)
               .HasColumnName("ai_cost")
               .HasColumnType("numeric(10,4)");

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
        // Versioning (Optimistic Concurrency )
        // ─────────────────────────────────────────────────────────────
        builder.Property(p => p.Version)
               .HasColumnName("version")
               .IsRequired()
               .HasDefaultValue(1)
               .IsConcurrencyToken(); //  important

        // ─────────────────────────────────────────────────────────────
        // Audit Fields
        // ─────────────────────────────────────────────────────────────
        builder.ConfigureAudit<PostMarketing, Guid>();

        // ─────────────────────────────────────────────────────────────
        // Indexes
        // ─────────────────────────────────────────────────────────────

        //  Unique tracking token
        builder.HasIndex(p => p.TrackingToken)
               .IsUnique()
               .HasDatabaseName("ix_post_tracking_token");

        //  Slug (for public URL)
        builder.HasIndex(p => p.Slug)
               .IsUnique()
               .HasDatabaseName("ix_post_marketing_slug");

        // Organizer dashboard queries
        builder.HasIndex(p => new { p.OrganizerId, p.Status, p.CreatedAt })
               .HasDatabaseName("ix_post_organizer_status_created");

        // Moderation queue (FIFO)
        builder.HasIndex(p => new { p.Status, p.SubmittedAt })
               .HasDatabaseName("ix_post_pending_queue")
               .HasFilter("status = 'Pending'");

        // Public feed (event-based)
        builder.HasIndex(p => new { p.EventId, p.PublishedAt })
               .HasDatabaseName("ix_post_event_published")
               .HasFilter("status = 'Published'");

        //  Global feed (important for homepage)
        builder.HasIndex(p => new { p.Status, p.PublishedAt })
               .HasDatabaseName("ix_post_global_feed")
               .HasFilter("status = 'Published'");
    }
}