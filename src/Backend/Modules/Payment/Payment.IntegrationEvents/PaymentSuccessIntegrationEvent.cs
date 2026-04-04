using Shared.Application.Abstractions.EventBus;

namespace Payment.IntegrationEvents;

public sealed record PaymentSuccessIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaidAtUtc { get; init; }

    public Guid PaymentTransactionId { get; init; }
    public Guid UserId { get; init; }
    public PaymentReferenceType ReferenceType { get; init; }
    public Guid ReferenceId { get; init; }

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

        PaymentTransactionId = Guid.Empty;
        UserId = Guid.Empty;
        ReferenceType = PaymentReferenceType.TicketOrder;
        ReferenceId = orderId;
    }

    public PaymentSuccessIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentTransactionId,
        Guid userId,
        PaymentReferenceType referenceType,
        Guid referenceId,
        decimal amount,
        DateTime paidAtUtc,
        Guid? orderId = null)
        : base(id, occurredOnUtc)
    {
        PaymentTransactionId = paymentTransactionId;
        UserId = userId;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Amount = amount;
        PaidAtUtc = paidAtUtc;

        OrderId = orderId ?? (referenceType == PaymentReferenceType.TicketOrder ? referenceId : Guid.Empty);
    }
}
