using Shared.Application.Abstractions.Messaging;

public record GetTransactionSummaryByEventQuery(Guid EventId) : IQuery<EventTransactionSummaryDto>;

public record EventTransactionSummaryDto(
    Guid EventId,
    int TotalTransactions,
    int CompletedCount,
    int FailedCount,
    int RefundedCount,
    decimal WalletPayAmount,
    decimal DirectPayAmount);