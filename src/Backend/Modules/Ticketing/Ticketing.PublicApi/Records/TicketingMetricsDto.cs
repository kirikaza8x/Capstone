namespace Ticketing.PublicApi.Records;

public sealed record TicketingMetricsDto(
    decimal TotalRevenue,
    double RevenueGrowthRate,
    int TotalTicketsSold);
