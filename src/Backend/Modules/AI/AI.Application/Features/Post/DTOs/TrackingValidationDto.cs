namespace Marketing.Application.Posts.Dtos;

public record TrackingValidationDto(
    bool IsValid,
    Guid? PostId,
    Guid? EventId,
    string? EventCode,
    string? RedirectUrl
);