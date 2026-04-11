namespace Payment.Application.Features.Vnpay.DTOs;

public record EventRevenueDto(
    Guid EventId,
    decimal Revenue
);

public record OrganizerRevenueSummaryDto(
    Guid OrganizerId,
    decimal GrossRevenue,
    decimal TotalRefunds,
    decimal NetRevenue,
    int EventCount,
    int CompletedEventCount,
    int ActiveEventCount,
    int UpcomingEventCount);

public record OrganizerRevenuePerEventItemDto(
    Guid EventId,
    string EventName,
    decimal GrossRevenue,
    decimal NetRevenue,
    decimal RefundAmount,
    decimal RefundRate,
    string Status);

public record OrganizerRevenuePerEventDto(
    Guid OrganizerId,
    IReadOnlyList<OrganizerRevenuePerEventItemDto> PerEvent);
