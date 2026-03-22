using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.Commands.MassRefundByEvent;

public record MassRefundByEventCommand(
    Guid EventId,
    Guid AdminId
) : ICommand<MassRefundResult>;

public record MassRefundResult(
    Guid EventId,
    int TotalFound,
    int Succeeded,
    int Skipped,
    int Failed,
    IReadOnlyList<MassRefundItemResult> Items
);

public record MassRefundItemResult(
    Guid PaymentTransactionId,
    Guid UserId,
    decimal Amount,
    bool Success,
    string? FailureReason
);