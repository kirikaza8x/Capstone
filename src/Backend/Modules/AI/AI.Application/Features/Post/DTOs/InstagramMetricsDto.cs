using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Dtos;

public sealed record InstagramMetricsDto
{
    public string ExternalPostId { get; init; } = string.Empty;
    public string ExternalUrl { get; init; } = string.Empty;
    public int Likes { get; init; }
    public int Comments { get; init; }
    public long Reach { get; init; }
    public long Saves { get; init; }
    public long Shares { get; init; }
    public int BuyCount { get; init; }
    public int ClickCount { get; init; }
    public int TicketsSold { get; init; }
    public double ConversionRate { get; init; }
    public string ConversionRateFormatted { get; init; } = string.Empty;
    public double EngagementRate { get; init; }
    public string EngagementRateFormatted { get; init; } = string.Empty;
    public DateTime FetchedAt { get; init; }
}
public class InstagramPageMetricsDto
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountUrl { get; set; } = string.Empty;
    public InstagramPeriod Period { get; set; }

    // Community
    public long FollowersCount { get; set; }
    public long Follows { get; set; }       // gained in period
    public long Unfollows { get; set; }     // lost in period

    // Reach & Visibility
    public long Reach { get; set; }         // unique accounts that saw content
    public long Views { get; set; }         // total content plays/displays (replaces Impressions)

    // Engagement
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public long Saves { get; set; }
    public long Reposts { get; set; }
    public long TotalInteractions { get; set; }
    public long AccountsEngaged { get; set; }

    public DateTime FetchedAt { get; set; }
}