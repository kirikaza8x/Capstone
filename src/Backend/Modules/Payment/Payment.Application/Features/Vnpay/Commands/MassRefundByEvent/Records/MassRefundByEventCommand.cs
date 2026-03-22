using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.Commands.MassRefundBySession;

public record MassRefundBySessionCommand(
    Guid EventSessionId,
    Guid AdminId
) : ICommand<MassRefundResult>;

public record MassRefundResult(
    Guid EventSessionId,
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
