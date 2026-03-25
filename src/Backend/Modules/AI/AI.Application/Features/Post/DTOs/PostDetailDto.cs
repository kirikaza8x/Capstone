namespace Marketing.Application.Posts.Dtos;

public record PostDetailDto(
    Guid PostId,
    Guid EventId,
    Guid OrganizerId,
    string Title,
    string Body,
    string? ImageUrl,
    string Status,
    string Platform,
    string? PromptUsed,
    string? AiModel,
    int? AiTokensUsed,
    string? RejectionReason,
    DateTime? PublishedAt,
    string TrackingToken,
    string? ExternalPostUrl,
    int Version,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    // Permissions for UI
    bool CanEdit,
    bool CanSubmit,
    bool CanPublish,
    bool CanArchive
);