using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Reports.GetOrganizerRevenueSummary;

public sealed record GetOrganizerRevenueSummaryQuery()
    : IQuery<OrganizerRevenueSummaryResponse>;

public sealed record OrganizerRevenueSummaryResponse(
    Guid OrganizerId,
    decimal GrossRevenue,
    decimal TotalDiscount,
    decimal TotalRefunds,
    decimal NetRevenue,
    int EventCount,
    int CompletedEventCount,
    int ActiveEventCount,
    int UpcomingEventCount);
