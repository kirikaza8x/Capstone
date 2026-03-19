// using Shared.Domain.DDD;

// namespace Users.Domain.Entities
// {
//     public class WalletTransaction : Entity<Guid>
//     {
//         public Guid WalletId { get; private set; }
//         public TransactionType Type { get; private set; }
//         public TransactionDirection Direction { get; private set; }
//         public decimal Amount { get; private set; }
//         public decimal BalanceBefore { get; private set; }
//         public decimal BalanceAfter { get; private set; }
//         public string? Note { get; private set; }
//         public TransactionStatus Status { get; private set; }

//         // EF Core constructor
//         private WalletTransaction() { }

//         public static WalletTransaction Create(
//             Guid walletId,
//             TransactionType type,
//             TransactionDirection direction,
//             decimal amount,
//             decimal balanceBefore,
//             decimal balanceAfter,
//             string? note = null)
//         {
//             var transaction = new WalletTransaction
//             {
//                 Id = Guid.NewGuid(),
//                 WalletId = walletId,
//                 Type = type,
//                 Direction = direction,
//                 Amount = amount,
//                 BalanceBefore = balanceBefore,
//                 BalanceAfter = balanceAfter,
//                 Note = note,
//                 Status = TransactionStatus.Pending,
//                 CreatedAt = DateTime.UtcNow
//             };


//             return transaction;
//         }

//         public void MarkCompleted()
//         {
//             Status = TransactionStatus.Completed;
//         }

//         public void MarkFailed(string? failureNote = null)
//         {
//             Status = TransactionStatus.Failed;
//             if (!string.IsNullOrEmpty(failureNote))
//                 Note = failureNote;
//         }
//     }

//     public enum TransactionType
//     {
//         Deposit,
//         Withdrawal,
//         Transfer,
//         Adjustment
//     }

//     public enum TransactionDirection
//     {
//         In,
//         Out
//     }

//     public enum TransactionStatus
//     {
//         Pending,
//         Completed,
//         Failed
//     }
// }
