
using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Dtos;

public sealed record FacebookMetricsDto
{
    public string ExternalPostId { get; init; } = string.Empty;
    public string ExternalUrl { get; init; } = string.Empty;
    public int Likes { get; init; }
    public int Comments { get; init; }
    public long Reach { get; init; }
    public long Clicks { get; init; }
    public DateTime FetchedAt { get; init; }
}


public class FacebookPageMetricsDto
{
    public string PageId { get; set; } = string.Empty;
    public string PageUrl { get; set; } = string.Empty;

    public FacebookPeriod Period { get; set; }

    // Audience Growth (28 Days)
    public long DailyUnfollowsUnique { get; set; } // Maps to: page_daily_unfollows_unique
    public long DailyFollowsUnique { get; set; }   // Maps to: page_daily_follows_unique

    // Traffic & Reach (28 Days)
    public long ViewsTotal { get; set; }           // Maps to: page_views_total
    public long ImpressionsUnique { get; set; }    // Maps to: page_impressions_unique

    // Engagement (28 Days)
    public long LikesTotal { get; set; }           // Maps to: page_actions_post_reactions_like_total
    public long PostEngagements { get; set; }      // Maps to: page_post_engagements

    public DateTime FetchedAt { get; set; }
}

