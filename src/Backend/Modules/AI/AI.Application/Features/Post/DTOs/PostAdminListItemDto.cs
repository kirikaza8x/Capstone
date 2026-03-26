namespace Marketing.Application.Posts.Dtos;

public record PostAdminListItemDto(
    Guid PostId,
    Guid EventId,
    string EventTitle,      // Fetched via Events.PublicApi
    Guid OrganizerId,
    string OrganizerName,   // Fetched via Users.PublicApi
    string Title,
    string Status,
    string Platform,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? RejectionReason
);