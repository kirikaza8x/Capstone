using Shared.Domain.DDD;

namespace Marketing.Domain.Entities;

/// <summary>
/// Stores Facebook insights (impressions, clicks, reactions) for a dispatched post.
/// One row per post per fetch — we keep history so the organizer can see trends over time.
/// The stats cron upserts by (PostMarketingId, FetchedDate) so you get one snapshot per day.
/// </summary>
public sealed class SocialPostAnalytics : Entity<Guid>
{
    // =========================================================
    // Identity
    // =========================================================

    public Guid PostMarketingId { get; private set; }

    /// <summary>
    /// Facebook's post ID, e.g. "123456789_987654321".
    /// Stored here redundantly so the cron can query FB without joining PostMarketing.
    /// </summary>
    public string ExternalPostId { get; private set; } = string.Empty;

    public string Platform { get; private set; } = string.Empty;

    // =========================================================
    // Facebook Insights fields
    // (FB Graph API: GET /{post-id}/insights?metric=...)
    // =========================================================

    /// <summary>post_impressions — total times the post entered a person's screen</summary>
    public long Impressions { get; private set; }

    /// <summary>post_clicks — total clicks on the post (link, photo, etc.)</summary>
    public long Clicks { get; private set; }

    /// <summary>post_reactions_by_type_total summed across all reaction types</summary>
    public long Reactions { get; private set; }

    /// <summary>post_shares</summary>
    public long Shares { get; private set; }

    /// <summary>post_video_views (0 for non-video posts)</summary>
    public long VideoViews { get; private set; }

    /// <summary>
    /// Reach = unique accounts that saw the post (post_impressions_unique).
    /// Different from Impressions which counts repeat views.
    /// </summary>
    public long Reach { get; private set; }

    // =========================================================
    // Snapshot metadata
    // =========================================================

    /// <summary>UTC date this row represents. One row per day per post.</summary>
    public DateOnly FetchedDate { get; private set; }

    public DateTime FetchedAt { get; private set; }

    // =========================================================
    // EF Core
    // =========================================================

    private SocialPostAnalytics() { }

    // =========================================================
    // Factory
    // =========================================================

    public static SocialPostAnalytics Create(
        Guid postMarketingId,
        string externalPostId,
        string platform,
        long impressions,
        long clicks,
        long reactions,
        long shares,
        long videoViews,
        long reach)
    {
        if (postMarketingId == Guid.Empty)
            throw new ArgumentException("PostMarketingId is required.", nameof(postMarketingId));

        if (string.IsNullOrWhiteSpace(externalPostId))
            throw new ArgumentException("ExternalPostId is required.", nameof(externalPostId));

        var now = DateTime.UtcNow;

        return new SocialPostAnalytics
        {
            Id = Guid.NewGuid(),
            PostMarketingId = postMarketingId,
            ExternalPostId = externalPostId.Trim(),
            Platform = platform.Trim().ToLowerInvariant(),
            Impressions = impressions,
            Clicks = clicks,
            Reactions = reactions,
            Shares = shares,
            VideoViews = videoViews,
            Reach = reach,
            FetchedDate = DateOnly.FromDateTime(now),
            FetchedAt = now,
            CreatedAt = now
        };
    }

    // =========================================================
    // Behaviour: allow updating the same-day row if cron runs twice
    // =========================================================

    public void Update(
        long impressions,
        long clicks,
        long reactions,
        long shares,
        long videoViews,
        long reach)
    {
        Impressions = impressions;
        Clicks = clicks;
        Reactions = reactions;
        Shares = shares;
        VideoViews = videoViews;
        Reach = reach;
        FetchedAt = DateTime.UtcNow;
        ModifiedAt = FetchedAt;
    }
}