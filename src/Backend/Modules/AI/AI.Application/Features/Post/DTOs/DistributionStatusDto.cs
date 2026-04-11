using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Dtos;



public record DistributionStatusDto(
    Guid Id,
    string Platform,
    string Status,
    string? ExternalUrl,
    string? ExternalPostId,
    string? PlatformMetadata,
    string? ErrorMessage,
    DateTime? SentAt
);


public class ExternalDistributionDto
{
    public Guid Id { get; init; }
    public Guid PostMarketingId { get; init; }
    public ExternalPlatform Platform { get; init; }
    public string ExternalUrl { get; init; } = string.Empty;
    public string? ExternalPostId { get; init; }
    public string? PlatformMetadata { get; init; }
    public DistributionStatus Status { get; init; } = DistributionStatus.Pending;
    public DateTime? SentAt { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}


public class ExternalDistributionDetailDto
{
    public Guid Id { get; init; }
    public Guid PostMarketingId { get; init; }
    public ExternalPlatform Platform { get; init; }
    public string ExternalUrl { get; init; } = string.Empty;
    public string? ExternalPostId { get; init; }
    public string? PlatformMetadata { get; init; }
    public DistributionStatus Status { get; init; } = DistributionStatus.Pending;
    public DateTime? SentAt { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}
