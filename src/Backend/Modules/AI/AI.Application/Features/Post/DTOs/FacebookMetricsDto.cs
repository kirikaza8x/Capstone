
namespace Marketing.Application.Posts.Dtos;

public sealed record FacebookMetricsDto
{
    public string ExternalPostId { get; init; } = string.Empty;
    public string ExternalUrl { get; init; } = string.Empty;
    public int Likes { get; init; }
    public int Comments { get; init; }
    public int Shares { get; init; }
    public int Impressions { get; init; }
    public int Reach { get; init; }
    public int Clicks { get; init; }
    public DateTime FetchedAt { get; init; }
}