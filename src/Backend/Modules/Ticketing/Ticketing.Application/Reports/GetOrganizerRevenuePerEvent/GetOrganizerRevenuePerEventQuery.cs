using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Reports.GetOrganizerRevenuePerEvent;

public sealed record GetOrganizerRevenuePerEventQuery(bool ByNet = false)
    : IQuery<OrganizerRevenuePerEventResponse>;

public sealed record OrganizerRevenuePerEventItemResponse(
    Guid EventId,
    string EventName,
    decimal GrossRevenue,
    decimal TotalDiscount,
    decimal RefundAmount,
    decimal NetRevenue,
    decimal DiscountRate,
    string Status);

public sealed record OrganizerRevenuePerEventResponse(
    Guid OrganizerId,
    IReadOnlyList<OrganizerRevenuePerEventItemResponse> PerEvent);
