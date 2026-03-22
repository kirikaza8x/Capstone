using Payments.Domain.Enums;

namespace Payments.Application.DTOs.Wallet;

public record WalletDto(
    Guid Id,
    Guid UserId,
    decimal Balance,
    WalletStatus Status,
    DateTime? CreatedAt
);

public record WalletTransactionDto(
    Guid Id,
    TransactionType Type,
    TransactionDirection Direction,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    TransactionStatus Status,
    string? Note,
    DateTime? CreatedAt
);

public record WalletWithTransactionsDto(
    Guid Id,
    Guid UserId,
    decimal Balance,
    WalletStatus Status,
    IReadOnlyList<WalletTransactionDto> Transactions,
    DateTime? CreatedAt
);