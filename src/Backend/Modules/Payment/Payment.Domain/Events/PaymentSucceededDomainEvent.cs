using Payment.Domain.Enums;
using Shared.Domain.DDD;

namespace Payment.Domain.Events;

public sealed record PaymentSucceededDomainEvent(
    Guid PaymentTransactionId,
    Guid UserId,
    PaymentReferenceType ReferenceType,
    Guid ReferenceId,
    Guid? OrderId,
    decimal Amount,
    DateTime CompletedAtUtc
) : DomainEvent;
