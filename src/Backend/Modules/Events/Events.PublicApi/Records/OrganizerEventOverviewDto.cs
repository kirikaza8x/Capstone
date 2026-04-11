namespace Events.PublicApi.Records;

public sealed record OrganizerEventOverviewDto(
    Guid EventId,
    string EventName,
    string Status,
    DateTime? EventStartAt,
    DateTime? EventEndAt);
