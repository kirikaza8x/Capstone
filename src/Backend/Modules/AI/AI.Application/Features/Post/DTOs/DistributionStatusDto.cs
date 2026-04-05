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