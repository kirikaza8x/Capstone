using Shared.Domain.DDD;

namespace Payments.Domain.Events;

public sealed record PaymentSucceededDomainEvent(
    Guid PaymentTransactionId,
    Guid OrderId,
    decimal Amount,
    DateTime CompletedAtUtc
) : DomainEvent;