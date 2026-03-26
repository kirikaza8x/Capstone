using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Dtos;

public record PostDto(
    Guid Id,
    Guid EventId,
    Guid OrganizerId,
    string Title,
    string Body,
    string? ImageUrl,
    string Status,          // enum as string for API
    string Platform,        // enum as string for API
    string? RejectionReason,
    DateTime? PublishedAt,
    string TrackingToken,
    string? ExternalPostUrl,
    int Version,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);