using Shared.Application.Abstractions.EventBus;

namespace Payment.IntegrationEvents;

public sealed record PaymentSuccessIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PaymentTransactionId,
    Guid UserId,
    PaymentReferenceType ReferenceType,
    Guid ReferenceId,
    decimal Amount,
    DateTime PaidAtUtc,
    Guid OrderId
) : IntegrationEvent(Id, OccurredOnUtc);
