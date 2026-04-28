namespace AI.Application.Features.Post.DTOs;

public sealed record ThreadsMetricsDto
{
    public string ExternalPostId { get; init; } = string.Empty;
    public string ExternalUrl { get; init; } = string.Empty;
    public long Views { get; init; }
    public int Likes { get; init; }
    public int Replies { get; init; }
    public int Reposts { get; init; }
    public int Quotes { get; init; }
    public int Shares { get; init; }
    public int BuyCount { get; init; }
    public int ClickCount { get; init; }
    public double ConversionRate { get; init; }
    public string ConversionRateFormatted { get; init; } = string.Empty;
    public double EngagementRate { get; init; }
    public string EngagementRateFormatted { get; init; } = string.Empty;
    public DateTime FetchedAt { get; init; }
}

public sealed record ThreadsAccountMetricsDto
{
    public string UserId { get; init; } = string.Empty;
    public long Views { get; init; }
    public int Likes { get; init; }
    public int Replies { get; init; }
    public int Reposts { get; init; }
    public int Quotes { get; init; }
    public long FollowersCount { get; init; }
    public DateTime FetchedAt { get; init; }
}
