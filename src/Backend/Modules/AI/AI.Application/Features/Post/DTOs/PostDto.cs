using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Dtos;

public class PostDto
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public Guid OrganizerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public DateTime? PublishedAt { get; init; }
    public string TrackingToken { get; init; } = string.Empty;
    public int Version { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public IReadOnlyList<DistributionStatusDto> Distributions { get; init; } 
        = new List<DistributionStatusDto>();
}
