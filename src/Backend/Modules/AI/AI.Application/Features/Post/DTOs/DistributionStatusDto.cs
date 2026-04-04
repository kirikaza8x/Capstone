using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Dtos;

public record DistributionStatusDto(
    ExternalPlatform Platform,
    string Status,              // "Pending", "Sent", "Failed"
    string? ExternalUrl,
    string? ExternalPostId,
    DateTime? SentAt,
    string? ErrorMessage
);