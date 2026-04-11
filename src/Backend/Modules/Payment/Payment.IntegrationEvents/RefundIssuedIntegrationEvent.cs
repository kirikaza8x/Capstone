using Shared.Application.Abstractions.EventBus;

namespace Payment.IntegrationEvents;

public sealed record RefundIssuedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid OrderTicketId { get; init; }
    public Guid EventSessionId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public DateTime RefundedAtUtc { get; init; }

    public RefundIssuedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid orderId,
        Guid orderTicketId,
        Guid eventSessionId,
        Guid userId,
        decimal amount,
        DateTime refundedAtUtc)
        : base(id, occurredOnUtc)
    {
        OrderId = orderId;
        OrderTicketId = orderTicketId;
        EventSessionId = eventSessionId;
        UserId = userId;
        Amount = amount;
        RefundedAtUtc = refundedAtUtc;
    }
}
