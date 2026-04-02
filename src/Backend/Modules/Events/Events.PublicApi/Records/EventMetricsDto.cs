namespace Events.PublicApi.Records;

public sealed record EventMetricsDto(
    int TotalEvents,
    int LiveEventsNow);
