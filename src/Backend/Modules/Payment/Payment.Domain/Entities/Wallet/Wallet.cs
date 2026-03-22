using Payments.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities;

public class Wallet : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public WalletStatus Status { get; private set; }

    public ICollection<WalletTransaction> Transactions { get; private set; }
        = new List<WalletTransaction>();

    private Wallet() { }

    public static Wallet Create(Guid userId, decimal initialBalance = 0)
    {
        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative.", nameof(initialBalance));

        return new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = initialBalance,
            Status = WalletStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public WalletTransaction Credit(decimal amount, string? note = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Credit amount must be positive.", nameof(amount));

        var before = Balance;
        Balance += amount;

        var txn = WalletTransaction.Create(
            walletId: Id,
            type: TransactionType.Deposit,
            direction: TransactionDirection.In,
            amount: amount,
            balanceBefore: before,
            balanceAfter: Balance,
            note: note
        );

        Transactions.Add(txn);
        return txn;
    }

    public WalletTransaction Debit(decimal amount, string? note = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Debit amount must be positive.", nameof(amount));

        if (Balance < amount)
            throw new InvalidOperationException(
                $"Insufficient funds. Balance: {Balance:N0}, Required: {amount:N0}.");

        var before = Balance;
        Balance -= amount;

        var txn = WalletTransaction.Create(
            walletId: Id,
            type: TransactionType.Withdrawal,
            direction: TransactionDirection.Out,
            amount: amount,
            balanceBefore: before,
            balanceAfter: Balance,
            note: note
        );

        Transactions.Add(txn);
        return txn;
    }

    public WalletTransaction Refund(decimal amount, string? note = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Refund amount must be positive.", nameof(amount));

        var before = Balance;
        Balance += amount;

        var txn = WalletTransaction.Create(
            walletId: Id,
            type: TransactionType.Refund,
            direction: TransactionDirection.In,
            amount: amount,
            balanceBefore: before,
            balanceAfter: Balance,
            note: note
        );

        Transactions.Add(txn);
        return txn;
    }

    public void ChangeStatus(WalletStatus newStatus)
    {
        if (Status == WalletStatus.Closed)
            throw new InvalidOperationException("A closed wallet cannot change status.");

        Status = newStatus;
    }

    protected override void Apply(IDomainEvent @event) { }
}
