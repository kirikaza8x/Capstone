namespace Events.PublicApi.Records;

public sealed record OrderEventSummaryDto(
    Guid EventId,
    Guid OrganizerId,
    string EventTitle,
    string EventStatus,
    string? BannerUrl,
    string? Location,
    DateTime? EventStartAt);
