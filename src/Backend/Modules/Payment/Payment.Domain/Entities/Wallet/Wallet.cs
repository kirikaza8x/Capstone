using Payments.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities
{
    public class Wallet : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public decimal Balance { get; private set; }
        public WalletStatus Status { get; private set; }

        public ICollection<WalletTransaction> Transactions { get; private set; } =
            new List<WalletTransaction>();


        private Wallet() { }

        public static Wallet Create(Guid userId, decimal initialBalance = 0)
        {
            if (initialBalance < 0)
                throw new ArgumentException("Initial balance cannot be negative.");

            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = initialBalance,
                Status = WalletStatus.Active
            };

            return wallet;
        }

        public WalletTransaction Credit(decimal amount, string? note = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            var before = Balance;
            Balance += amount;

            var transaction = WalletTransaction.Create(
                Id,
                TransactionType.Deposit,
                TransactionDirection.In,
                amount,
                before,
                Balance,
                note
            );
            Transactions.Add(transaction);
            return transaction;
        }

        public WalletTransaction Debit(decimal amount, string? note = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds.");

            var before = Balance;
            Balance -= amount;

            var transaction = WalletTransaction.Create(
                Id,
                TransactionType.Withdrawal,
                TransactionDirection.Out,
                amount,
                before,
                Balance,
                note
            );

            Transactions.Add(transaction);
            return transaction;
        }

        public void ChangeStatus(WalletStatus newStatus)
        {
            Status = newStatus;
        }

        protected override void Apply(IDomainEvent @event) { }
    }


}
