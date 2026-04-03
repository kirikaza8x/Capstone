using Marketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Extensions;

namespace Marketing.Infrastructure.Persistence.Configs;

public class SocialPostAnalyticsConfiguration : IEntityTypeConfiguration<SocialPostAnalytics>
{
    public void Configure(EntityTypeBuilder<SocialPostAnalytics> builder)
    {
        builder.ToTable("social_post_analytics");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        builder.Property(a => a.PostMarketingId)
               .HasColumnName("post_marketing_id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(a => a.ExternalPostId)
               .HasColumnName("external_post_id")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(a => a.Platform)
               .HasColumnName("platform")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(a => a.Impressions)
               .HasColumnName("impressions");

        builder.Property(a => a.Clicks)
               .HasColumnName("clicks");

        builder.Property(a => a.Reactions)
               .HasColumnName("reactions");

        builder.Property(a => a.Shares)
               .HasColumnName("shares");

        builder.Property(a => a.VideoViews)
               .HasColumnName("video_views");

        builder.Property(a => a.Reach)
               .HasColumnName("reach");

        builder.Property(a => a.FetchedDate)
               .HasColumnName("fetched_date");

        builder.Property(a => a.FetchedAt)
               .HasColumnName("fetched_at")
               .HasColumnType("timestamp with time zone");

        builder.ConfigureAudit<SocialPostAnalytics, Guid>();

        builder.HasIndex(a => new { a.PostMarketingId, a.FetchedDate })
               .IsUnique()
               .HasDatabaseName("ix_social_analytics_post_day");
    }
}