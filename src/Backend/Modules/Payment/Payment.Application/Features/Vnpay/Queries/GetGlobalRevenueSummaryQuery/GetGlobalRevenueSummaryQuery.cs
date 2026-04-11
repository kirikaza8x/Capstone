using Shared.Application.Abstractions.Messaging;

public record GetGlobalRevenueSummaryQuery : IQuery<GlobalRevenueSummaryDto>;

public record GlobalRevenueSummaryDto(
    decimal GrossRevenue,
    decimal TotalRefunds,
    decimal NetRevenue,
    int EventCount);