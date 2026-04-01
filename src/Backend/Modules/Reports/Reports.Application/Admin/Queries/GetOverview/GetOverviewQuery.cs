using Shared.Application.Abstractions.Messaging;

namespace Reports.Application.Admin.Queries.GetOverview;

public sealed record GetOverviewQuery() : IQuery<OverviewResponse>;

public sealed record OverviewResponse(
    AdminKpisDto Kpis,
    UserDistributionDto UserDistribution);

public sealed record AdminKpisDto(
    TotalRevenueDto TotalRevenue,
    ActiveUsersDto ActiveUsers,
    EventsSummaryDto Events,
    TicketsSoldSummaryDto TicketsSold);

public sealed record TotalRevenueDto(
    decimal Value,
    double MonthlyGrowthRate,
    bool IsPositiveGrowth);

public sealed record ActiveUsersDto(int Total);

public sealed record EventsSummaryDto(
    int Total,
    int LiveNow);

public sealed record TicketsSoldSummaryDto(int Total);

public sealed record UserDistributionDto(
    UserRoleStatDto Attendees,
    UserRoleStatDto Organizers,
    double GrowthRate);

public sealed record UserRoleStatDto(
    int Count,
    double Percentage);
