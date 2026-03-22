// using Microsoft.Extensions.Logging;
// using Payment.Application.Features.VnPay.Dtos;
// using Payment.Domain.Enums;
// using Payments.Application.Features.Commands.MassRefundByEvent;
// using Payments.Domain.Entities;
// using Payments.Domain.Enums;
// using Payments.Domain.Repositories;
// using Payments.Domain.UOW;
// using Shared.Application.Abstractions.Messaging;
// using Shared.Domain.Abstractions;

// namespace Payments.Application.Features.Payments.Commands.MassRefundByEvent;

// public class MassRefundByEventCommandHandler(
//     IPaymentTransactionRepository transactionRepository,
//     IWalletRepository walletRepository,
//     IPaymentUnitOfWork unitOfWork,
//     ILogger<MassRefundByEventCommandHandler> logger)
//     : ICommandHandler<MassRefundByEventCommand, MassRefundResultDto>
// {
//     private const int BatchSize = 50;

//     public async Task<Result<MassRefundResultDto>> Handle(
//         MassRefundByEventCommand command,
//         CancellationToken cancellationToken)
//     {
//         // 1. Load all completed DirectPay + WalletPay for this event
//         var transactions = (await transactionRepository
//             .GetAllCompletedByEventIdAsync(command.EventId, cancellationToken))
//             .ToList();

//         if (transactions.Count == 0)
//         {
//             logger.LogInformation(
//                 "MassRefund: no completed transactions for EventId={EventId}",
//                 command.EventId);

//             return Result.Success(new MassRefundResultDto(command.EventId, 0, 0, 0, 0, []));
//         }

//         logger.LogInformation(
//             "MassRefund started: EventId={EventId}, Total={Total}",
//             command.EventId, transactions.Count);

//         var items = new List<MassRefundItemResult>();
//         int succeeded = 0, skipped = 0, failed = 0;

//         foreach (var batch in transactions.Chunk(BatchSize))
//         {
//             // 2. Pre-load all wallets for this batch in one query — avoids N+1
//             var userIds = batch.Select(t => t.UserId).Distinct().ToList();
//             var walletMap = (await walletRepository.GetByUserIdsAsync(userIds, cancellationToken))
//                 .ToDictionary(w => w.UserId);

//             // 3. Create wallets in bulk for users who don't have one yet
//             var missingUserIds = userIds.Except(walletMap.Keys).ToList();
//             if (missingUserIds.Count > 0)
//             {
//                 var newWallets = missingUserIds
//                     .Select(uid => Wallet.Create(uid, initialBalance: 0))
//                     .ToList();

//                 await walletRepository.BulkInsertAsync(newWallets, cancellationToken);

//                 foreach (var w in newWallets)
//                     walletMap[w.UserId] = w;

//                 logger.LogInformation(
//                     "MassRefund: created {Count} new wallets for EventId={EventId}",
//                     newWallets.Count, command.EventId);
//             }

//             // 4. Process each transaction in memory — collect mutations for bulk update
//             var txnsToUpdate = new List<PaymentTransaction>();
//             var walletsToUpdate = new List<Wallet>();

//             foreach (var txn in batch)
//             {
//                 // Already refunded — skip, count as skipped not failed
//                 if (txn.InternalStatus == PaymentInternalStatus.Refunded)
//                 {
//                     skipped++;
//                     items.Add(new MassRefundItemResult(
//                         txn.Id, txn.UserId, txn.Amount, true, "Already refunded"));
//                     continue;
//                 }

//                 try
//                 {
//                     var wallet = walletMap[txn.UserId];

//                     // Reactivate suspended wallets silently — a refund should always land
//                     if (wallet.Status == WalletStatus.Suspended)
//                         wallet.ChangeStatus(WalletStatus.Active);

//                     // Credit wallet — new WalletTransaction child row tracked by EF change tracker
//                     var walletTxn = wallet.Credit(
//                         txn.Amount,
//                         $"Mass refund | EventId={command.EventId} | TxnId={txn.Id}"
//                     );
//                     walletTxn.MarkCompleted();

//                     // Mutate payment transaction state in memory
//                     txn.MarkRefunded();

//                     txnsToUpdate.Add(txn);
//                     walletsToUpdate.Add(wallet);

//                     succeeded++;
//                     items.Add(new MassRefundItemResult(
//                         txn.Id, txn.UserId, txn.Amount, true, null));
//                 }
//                 catch (Exception ex)
//                 {
//                     // One item failure must never block the rest of the batch
//                     failed++;
//                     logger.LogError(ex,
//                         "MassRefund item failed for TxnId={TxnId}, UserId={UserId}",
//                         txn.Id, txn.UserId);
//                     items.Add(new MassRefundItemResult(
//                         txn.Id, txn.UserId, txn.Amount, false, ex.Message));
//                 }
//             }

//             // 5. Persist batch — skip if nothing to write (e.g. entire batch was already refunded)
//             if (txnsToUpdate.Count == 0)
//                 continue;

//             // Wrap in a transaction so SaveChanges (WalletTransaction inserts) +
//             // BulkUpdate (wallet balances + payment statuses) are atomic per batch.
//             // BulkExtensions enlists in the ambient IDbContextTransaction automatically
//             // since it shares the same DbContext instance.
//             await unitOfWork.BeginTransactionAsync(cancellationToken);
//             try
//             {
//                 // SaveChanges first — flushes new WalletTransaction rows tracked by EF.
//                 // Must happen before BulkUpdate so FK constraints are satisfied.
//                 // CommitTransactionAsync also calls SaveChangesAsync internally but by
//                 // then there is nothing left dirty — the second call is a harmless no-op.
//                 await unitOfWork.SaveChangesAsync(cancellationToken);

//                 // BulkUpdate goes directly to DB inside the ambient transaction —
//                 // no extra SaveChanges needed, committed when CommitTransactionAsync fires.
//                 await transactionRepository.BulkUpdateAsync(txnsToUpdate, cancellationToken);
//                 await walletRepository.BulkUpdateAsync(walletsToUpdate, cancellationToken);

//                 await unitOfWork.CommitTransactionAsync(cancellationToken);

//                 logger.LogInformation(
//                     "MassRefund batch committed: EventId={EventId}, Succeeded={S}, Skipped={Sk}, Failed={F}",
//                     command.EventId, succeeded, skipped, failed);
//             }
//             catch (Exception ex)
//             {
//                 await unitOfWork.RollbackTransactionAsync(cancellationToken);

//                 // Batch-level failure — mark every item in this batch as failed
//                 // and unwind the in-memory succeeded count for items we just rolled back
//                 logger.LogError(ex,
//                     "MassRefund batch rolled back for EventId={EventId}", command.EventId);

//                 foreach (var txn in txnsToUpdate)
//                 {
//                     // Find and replace the optimistic success entry we added above
//                     var idx = items.FindLastIndex(i => i.PaymentTransactionId == txn.Id && i.Success);
//                     if (idx >= 0)
//                     {
//                         items[idx] = items[idx] with
//                         {
//                             Success = false,
//                             FailureReason = $"Batch commit failed: {ex.Message}"
//                         };
//                     }
//                 }

//                 succeeded -= txnsToUpdate.Count;
//                 failed += txnsToUpdate.Count;
//             }
//         }

//         logger.LogInformation(
//             "MassRefund complete: EventId={EventId}, TotalFound={Total}, " +
//             "Succeeded={S}, Skipped={Sk}, Failed={F}",
//             command.EventId, transactions.Count, succeeded, skipped, failed);

//         return Result.Success(new MassRefundResultDto(
//             EventId: command.EventId,
//             TotalFound: transactions.Count,
//             Succeeded: succeeded,
//             Skipped: skipped,
//             Failed: failed,
//             Items: items
//         ));
//     }
// }
