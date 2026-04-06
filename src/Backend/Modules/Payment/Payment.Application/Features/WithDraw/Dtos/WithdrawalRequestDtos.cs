using Payment.Domain.Enums;

namespace Payments.Application.Features.WithdrawalRequests.Dtos;

/// <summary>User-facing list item — no admin-sensitive fields.</summary>
public sealed record WithdrawalRequestListItemDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string? ReceiverName { get; init; }
    public string BankAccountNumber { get; init; } = default!;
    public string BankName { get; init; } = default!;
    public WithdrawalRequestStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}

/// <summary>User-facing detail — includes their own notes and rejection reason.</summary>
public sealed record WithdrawalRequestDetailDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string? ReceiverName { get; init; }
    public string BankAccountNumber { get; init; } = default!;
    public string BankName { get; init; } = default!;
    public WithdrawalRequestStatus Status { get; init; }
    public string? Notes { get; init; }
    public string? AdminNote { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}

/// <summary>Admin-facing list item — includes user identity for the queue view.</summary>
public sealed record WithdrawalRequestAdminListItemDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid WalletId { get; init; }
    public string? ReceiverName { get; init; }
    public decimal Amount { get; init; }
    public string BankAccountNumber { get; init; } = default!;
    public string BankName { get; init; } = default!;
    public WithdrawalRequestStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}

/// <summary>Admin-facing detail — full picture including wallet transaction linkage.</summary>
public sealed record WithdrawalRequestAdminDetailDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public string? ReceiverName { get; init; }
    public string BankAccountNumber { get; init; } = default!;
    public string BankName { get; init; } = default!;
    public WithdrawalRequestStatus Status { get; init; }
    public string? Notes { get; init; }
    public string? AdminNote { get; init; }
    public Guid? WalletTransactionId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}
