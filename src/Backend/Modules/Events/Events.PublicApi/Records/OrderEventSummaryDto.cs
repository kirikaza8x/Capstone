namespace Events.PublicApi.Records;

public sealed record OrderEventSummaryDto(
    Guid EventId,
    string EventTitle,
    string? BannerUrl,
    string? Location,
    DateTime? EventStartAt);
