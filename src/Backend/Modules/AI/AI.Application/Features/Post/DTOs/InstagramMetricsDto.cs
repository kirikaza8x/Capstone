using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Dtos;

public sealed record InstagramMetricsDto
{
    public string ExternalPostId { get; init; } = string.Empty;
    public string ExternalUrl { get; init; } = string.Empty;
    public int Likes { get; init; }
    public int Comments { get; init; }
    public long Reach { get; init; }      // post_impressions_unique
    public long Saves { get; init; }      // post_saves (Instagram-specific)
    public DateTime FetchedAt { get; init; }
}

public sealed class InstagramPageMetricsDto
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountUrl { get; set; } = string.Empty;
    public InstagramPeriod Period { get; set; }

    // Audience Growth (28 Days)
    public long FollowersCount { get; set; }           // follower_count
    // public long DailyFollows { get; set; }             // online_followers (approximation)

    // Reach & Impressions (28 Days)
    // public long Impressions { get; set; }              // impressions
    public long Reach { get; set; }                    // reach

    // Engagement (28 Days)
    public long ProfileViews { get; set; }             // profile_views
    public long Engagement { get; set; }               // engagement (likes + comments + saves)

    public DateTime FetchedAt { get; set; }
}