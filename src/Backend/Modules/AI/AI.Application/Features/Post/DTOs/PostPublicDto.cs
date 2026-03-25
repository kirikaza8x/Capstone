namespace Marketing.Application.Posts.Dtos;

public record PostPublicDto(
    Guid PostId,
    Guid EventId,
    string Title,
    string Body,
    string? ImageUrl,
    DateTime PublishedAt,
    string TrackingUrl      // Full URL with token for tracking
);