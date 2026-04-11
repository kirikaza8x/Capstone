using Payment.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities;

/// <summary>
/// Represents a user's request to withdraw funds from their wallet balance
/// to an external bank account. The actual bank transfer is performed manually
/// by an admin outside the system.
/// </summary>
public class WithdrawalRequest : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid WalletId { get; private set; }

    public string? Name { get; private set; }

    // ── Bank destination ──────────────────────────────────────────────────────
    public string BankAccountNumber { get; private set; } = default!;
    public string BankName { get; private set; } = default!;

    // ── Request details ───────────────────────────────────────────────────────
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    public WithdrawalRequestStatus Status { get; private set; }

    /// <summary>Set by admin when approving or rejecting the request.</summary>
    public string? AdminNote { get; private set; }

    /// <summary>Timestamp when the admin took action (approved / rejected / completed).</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>The wallet transaction created when the request is approved (balance hold).</summary>
    public Guid? WalletTransactionId { get; private set; }

    private WithdrawalRequest() { }

    // ── Factory ───────────────────────────────────────────────────────────────
    public static WithdrawalRequest Create(
        Guid userId,
        Guid walletId,
        string? name,
        string bankAccountNumber,
        string bankName,
        decimal amount,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(bankAccountNumber))
            throw new ArgumentException("Bank account number is required.", nameof(bankAccountNumber));

        if (string.IsNullOrWhiteSpace(bankName))
            throw new ArgumentException("Bank name is required.", nameof(bankName));

        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive.", nameof(amount));

        return new WithdrawalRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WalletId = walletId,
            Name = name?.Trim(),
            BankAccountNumber = bankAccountNumber.Trim(),
            BankName = bankName.Trim(),
            Amount = amount,
            Notes = notes?.Trim(),
            Status = WithdrawalRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ── State transitions ─────────────────────────────────────────────────────
    /// <summary>
    /// Admin approves the request. The caller is responsible for calling
    /// <see cref="Wallet.Debit"/> and passing the resulting transaction ID here
    /// so the balance is held immediately.
    /// </summary>
    public void Approve(Guid walletTransactionId, string? adminNote = null)
    {
        EnsureStatus(WithdrawalRequestStatus.Pending, nameof(Approve));

        WalletTransactionId = walletTransactionId;
        AdminNote = adminNote?.Trim();
        Status = WithdrawalRequestStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin rejects the request. No balance change occurs.
    /// </summary>
    public void Reject(string adminNote)
    {
        EnsureStatus(WithdrawalRequestStatus.Pending, nameof(Reject));

        if (string.IsNullOrWhiteSpace(adminNote))
            throw new ArgumentException("A rejection reason is required.", nameof(adminNote));

        AdminNote = adminNote.Trim();
        Status = WithdrawalRequestStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin marks the request as completed after performing the manual bank transfer.
    /// </summary>
    public void Complete(string? adminNote = null)
    {
        EnsureStatus(WithdrawalRequestStatus.Approved, nameof(Complete));

        AdminNote = adminNote?.Trim() ?? AdminNote;
        Status = WithdrawalRequestStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// User cancels their own request before the admin has acted on it.
    /// The caller is responsible for refunding the wallet balance if it was already debited.
    /// </summary>
    public void Cancel()
    {
        if (Status != WithdrawalRequestStatus.Pending)
            throw new InvalidOperationException(
                "Only pending withdrawal requests can be cancelled by the user.");

        Status = WithdrawalRequestStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
/// Admin marks the request as failed after the bank transfer could not be completed.
/// The caller is responsible for refunding the wallet balance.
/// </summary>
public void Fail(string adminNote)
{
    EnsureStatus(WithdrawalRequestStatus.Approved, nameof(Fail));

    if (string.IsNullOrWhiteSpace(adminNote))
        throw new ArgumentException("A failure reason is required.", nameof(adminNote));

    AdminNote = adminNote.Trim();
    Status = WithdrawalRequestStatus.Failed;
    ProcessedAt = DateTime.UtcNow;
}

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void EnsureStatus(WithdrawalRequestStatus expected, string operation)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Cannot {operation} a withdrawal request with status '{Status}'.");
    }

    protected override void Apply(IDomainEvent @event) { }
}
