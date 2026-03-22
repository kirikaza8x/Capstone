// using Microsoft.Extensions.Logging;
// using Payments.Domain.Entities;
// using Payments.Domain.Enums;
// using Payments.Domain.Repositories;
// using Payments.Domain.UOW;
// using Shared.Application.Abstractions.Messaging;
// using Shared.Domain.Abstractions;
// using Payment.Domain.Enums;

// namespace Payments.Application.Features.Refunds.Commands.MassRefundByEvent;

// public class MassRefundByEventCommandHandler(
//     IPaymentTransactionRepository transactionRepository,
//     IWalletRepository walletRepository,
//     IPaymentUnitOfWork unitOfWork,
//     ILogger<MassRefundByEventCommandHandler> logger)
//     : ICommandHandler<MassRefundByEventCommand, MassRefundResult>
// {
//     private const int BatchSize = 50;

//     public async Task<Result<MassRefundResult>> Handle(
//         MassRefundByEventCommand command, CancellationToken cancellationToken)
//     {
//         var pairs = (await transactionRepository
//             .GetAllCompletedItemsByEventIdAsync(command.EventId, cancellationToken))
//             .ToList();

//         if (pairs.Count == 0)
//         {
//             logger.LogInformation(
//                 "MassRefund: no completed items for EventId={EventId}", command.EventId);
//             return Result.Success(new MassRefundResult(command.EventId, 0, 0, 0, 0, []));
//         }

//         logger.LogInformation("MassRefund started: EventId={EventId}, Total={Total}",
//             command.EventId, pairs.Count);

//         var resultItems = new List<MassRefundItemResult>();
//         int succeeded = 0, skipped = 0, failed = 0;

//         foreach (var batch in pairs.Chunk(BatchSize))
//         {
//             // Pre-load wallets for this batch
//             var userIds = batch.Select(p => p.Transaction.UserId).Distinct().ToList();
//             var walletMap = (await walletRepository
//                 .GetByUserIdsAsync(userIds, cancellationToken))
//                 .ToDictionary(w => w.UserId);

//             // Bulk create missing wallets
//             var missingIds = userIds.Except(walletMap.Keys).ToList();
//             if (missingIds.Count > 0)
//             {
//                 var newWallets = missingIds
//                     .Select(uid => Wallet.Create(uid))
//                     .ToList();

//                 await walletRepository.BulkInsertAsync(newWallets, cancellationToken);

//                 foreach (var w in newWallets)
//                     walletMap[w.UserId] = w;
//             }

//             var txnsToUpdate = new List<PaymentTransaction>();
//             var walletsToUpdate = new List<Wallet>();

//             foreach (var (txn, item) in batch)
//             {
//                 // Skip already refunded silently
//                 if (item.InternalStatus == PaymentInternalStatus.Refunded)
//                 {
//                     skipped++;
//                     resultItems.Add(new MassRefundItemResult(
//                         txn.Id, txn.UserId, item.Amount, true, "Already refunded"));
//                     continue;
//                 }

//                 try
//                 {
//                     var wallet = walletMap[txn.UserId];

//                     if (wallet.Status == WalletStatus.Suspended)
//                         wallet.ChangeStatus(WalletStatus.Active);

//                     var walletTxn = wallet.Refund(
//                         item.Amount,
//                         $"Mass refund | EventId={command.EventId} | TxnId={txn.Id}"
//                     );
//                     walletTxn.MarkCompleted();

//                     item.MarkRefunded();

//                     if (txn.IsFullyRefunded())
//                         txn.MarkRefunded();

//                     txnsToUpdate.Add(txn);
//                     walletsToUpdate.Add(wallet);

//                     succeeded++;
//                     resultItems.Add(new MassRefundItemResult(
//                         txn.Id, txn.UserId, item.Amount, true, null));
//                 }
//                 catch (Exception ex)
//                 {
//                     failed++;
//                     logger.LogError(ex,
//                         "MassRefund item failed TxnId={TxnId}", txn.Id);
//                     resultItems.Add(new MassRefundItemResult(
//                         txn.Id, txn.UserId, item.Amount, false, ex.Message));
//                 }
//             }

//             if (txnsToUpdate.Count == 0) continue;

//             await unitOfWork.BeginTransactionAsync(cancellationToken);
//             try
//             {
//                 // Flush new WalletTransaction rows (EF-tracked via wallet.Refund())
//                 await unitOfWork.SaveChangesAsync(cancellationToken);

//                 // BulkUpdate goes directly to DB inside the ambient transaction
//                 await transactionRepository.BulkUpdateAsync(txnsToUpdate, cancellationToken);
//                 await walletRepository.BulkUpdateAsync(walletsToUpdate, cancellationToken);

//                 await unitOfWork.CommitTransactionAsync(cancellationToken);

//                 logger.LogInformation(
//                     "MassRefund batch committed: EventId={EventId}, S={S}, Sk={Sk}, F={F}",
//                     command.EventId, succeeded, skipped, failed);
//             }
//             catch (Exception ex)
//             {
//                 await unitOfWork.RollbackTransactionAsync(cancellationToken);

//                 logger.LogError(ex,
//                     "MassRefund batch rolled back: EventId={EventId}", command.EventId);

//                 foreach (var txn in txnsToUpdate)
//                 {
//                     var idx = resultItems.FindLastIndex(
//                         i => i.PaymentTransactionId == txn.Id && i.Success);
//                     if (idx >= 0)
//                         resultItems[idx] = resultItems[idx] with
//                         {
//                             Success = false,
//                             FailureReason = $"Batch commit failed: {ex.Message}"
//                         };
//                 }

//                 succeeded -= txnsToUpdate.Count;
//                 failed += txnsToUpdate.Count;
//             }
//         }

//         logger.LogInformation(
//             "MassRefund complete: EventId={EventId}, T={T}, S={S}, Sk={Sk}, F={F}",
//             command.EventId, pairs.Count, succeeded, skipped, failed);

//         return Result.Success(new MassRefundResult(
//             command.EventId, pairs.Count,
//             succeeded, skipped, failed, resultItems));
//     }
// }