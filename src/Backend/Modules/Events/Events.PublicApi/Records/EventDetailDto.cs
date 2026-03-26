namespace Events.PublicApi.Records;

public sealed record EventDetailDto(
    Guid EventId,
    Guid OrganizerId,
    string Title,
    string? Description,
    DateTime? EventStartAt,
    string? Location,
    IReadOnlyCollection<string> Hashtags,
    IReadOnlyCollection<string> Categories
);
