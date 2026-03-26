namespace Marketing.Application.Posts.Dtos;

public record PostPendingItemDto(
    Guid PostId,
    Guid EventId,
    string EventTitle,
    Guid OrganizerId,
    string OrganizerName,
    string Title,
    string Body,
    string? ImageUrl,
    string Platform,
    DateTime SubmittedAt,
    int Version
);