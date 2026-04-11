using Shared.Domain.DDD;

namespace Payments.Domain.Events;

public sealed record RefundIssuedDomainEvent(
    Guid PaymentTransactionId,
    Guid OrderId,
    Guid OrderTicketId,
    Guid EventSessionId,
    Guid UserId,
    decimal Amount,
    DateTime RefundedAtUtc
) : DomainEvent;
