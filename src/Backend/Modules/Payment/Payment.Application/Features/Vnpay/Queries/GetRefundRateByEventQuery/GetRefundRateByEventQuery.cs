using Shared.Application.Abstractions.Messaging;

public record GetRefundRateByEventQuery(Guid EventId) : IQuery<EventRefundRateDto>;
public record EventRefundRateDto(
    Guid EventId,
    decimal GrossRevenue,
    decimal TotalRefunds,
    decimal RefundRatePercent);