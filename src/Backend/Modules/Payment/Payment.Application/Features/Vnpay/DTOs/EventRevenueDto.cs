namespace Payments.Application.Features.Vnpay.DTOs;

public record EventRevenueDto(
    Guid EventId,
    decimal Revenue
);

public record OrganizerRevenueSummaryDto(
    Guid OrganizerId,
    decimal GrossRevenue,
    decimal TotalRefunds,
    decimal NetRevenue,
    int EventCount);

public record OrganizerRevenuePerEventDto(
    Guid OrganizerId,
    IReadOnlyList<EventRevenueDto> PerEvent);