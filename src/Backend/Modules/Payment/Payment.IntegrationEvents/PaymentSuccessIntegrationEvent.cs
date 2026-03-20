using Shared.Application.Abstractions.EventBus;

namespace Payment.IntegrationEvents;

public sealed record PaymentSuccessIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaidAtUtc { get; init; }

    public PaymentSuccessIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid orderId,
        decimal amount,
        DateTime paidAtUtc)
        : base(id, occurredOnUtc)
    {
        OrderId = orderId;
        Amount = amount;
        PaidAtUtc = paidAtUtc;
    }
}