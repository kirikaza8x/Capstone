namespace Ticketing.PublicApi.Records;

public sealed record TopEventTicketMetricsDto(
    Guid EventId,
    decimal TotalRevenue,
    int TicketsSold);
