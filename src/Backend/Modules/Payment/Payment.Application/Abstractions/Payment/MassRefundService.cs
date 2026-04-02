
using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Payments.Application.Features.Refunds.Commands.MassRefundBySession;

namespace Payments.Application.Features.Refunds.Services;

public class MassRefundService(
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<MassRefundService> logger)
{
    private const int BatchSize = 50;

    public async Task<MassRefundResult> ExecuteAsync(
        Guid scopeId,
        IEnumerable<(PaymentTransaction Transaction, BatchPaymentItem Item)> pairs,
        string refundNotePrefix,
        CancellationToken ct)
    {
        var allPairs = pairs.ToList();

        if (allPairs.Count == 0)
        {
            logger.LogInformation(
                "MassRefund: no items found for ScopeId={ScopeId}", scopeId);

            return new MassRefundResult(scopeId, 0, 0, 0, 0, []);
        }

        logger.LogInformation(
            "MassRefund started: ScopeId={ScopeId}, Total={Total}",
            scopeId, allPairs.Count);

        var resultItems = new List<MassRefundItemResult>();
        int succeeded = 0, skipped = 0, failed = 0;

        foreach (var batch in allPairs.Chunk(BatchSize))
        {
            // Pre-load all wallets for this batch — avoids N+1
            var userIds = batch
                .Select(p => p.Transaction.UserId)
                .Distinct()
                .ToList();

            var walletMap = (await walletRepository.GetByUserIdsAsync(userIds, ct))
                .ToDictionary(w => w.UserId);

            // Create missing wallets in bulk
            var missingIds = userIds.Except(walletMap.Keys).ToList();
            if (missingIds.Count > 0)
            {
                var newWallets = missingIds
                    .Select(uid => Wallet.Create(uid))
                    .ToList();

                await walletRepository.BulkInsertAsync(newWallets, ct);

                foreach (var w in newWallets)
                    walletMap[w.UserId] = w;
            }

            var txnsToUpdate = new List<PaymentTransaction>();
            var walletsToUpdate = new List<Wallet>();

            foreach (var (txn, item) in batch)
            {
                // Already refunded — skip silently
                if (item.InternalStatus == PaymentInternalStatus.Refunded)
                {
                    skipped++;
                    resultItems.Add(new MassRefundItemResult(
                        txn.Id, txn.UserId, item.Amount, true, "Already refunded"));
                    continue;
                }

                try
                {
                    var wallet = walletMap[txn.UserId];

                    if (wallet.Status == WalletStatus.Suspended)
                        wallet.ChangeStatus(WalletStatus.Active);

                    var walletTxn = wallet.Refund(
                        item.Amount,
                        $"{refundNotePrefix} | TxnId={txn.Id}");

                    walletTxn.MarkCompleted();

                    txn.MarkItemRefunded(item, txn.UserId);

                    txnsToUpdate.Add(txn);
                    walletsToUpdate.Add(wallet);

                    succeeded++;
                    resultItems.Add(new MassRefundItemResult(
                        txn.Id, txn.UserId, item.Amount, true, null));
                }
                catch (Exception ex)
                {
                    failed++;
                    logger.LogError(ex,
                        "MassRefund item failed TxnId={TxnId}", txn.Id);

                    resultItems.Add(new MassRefundItemResult(
                        txn.Id, txn.UserId, item.Amount, false, ex.Message));
                }
            }

            if (txnsToUpdate.Count == 0) continue;

            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                await unitOfWork.SaveChangesAsync(ct);
                await transactionRepository.BulkUpdateAsync(txnsToUpdate, ct);
                await walletRepository.BulkUpdateAsync(walletsToUpdate, ct);
                await unitOfWork.CommitTransactionAsync(ct);

                logger.LogInformation(
                    "MassRefund batch committed: ScopeId={ScopeId}, S={S}, Sk={Sk}, F={F}",
                    scopeId, succeeded, skipped, failed);
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackTransactionAsync(ct);

                logger.LogError(ex,
                    "MassRefund batch rolled back: ScopeId={ScopeId}", scopeId);

                // Flip succeeded items in this batch back to failed
                foreach (var txn in txnsToUpdate)
                {
                    var idx = resultItems.FindLastIndex(
                        i => i.PaymentTransactionId == txn.Id && i.Success);

                    if (idx >= 0)
                        resultItems[idx] = resultItems[idx] with
                        {
                            Success = false,
                            FailureReason = $"Batch commit failed: {ex.Message}"
                        };
                }

                succeeded -= txnsToUpdate.Count;
                failed += txnsToUpdate.Count;
            }
        }

        logger.LogInformation(
            "MassRefund complete: ScopeId={ScopeId}, T={T}, S={S}, Sk={Sk}, F={F}",
            scopeId, allPairs.Count, succeeded, skipped, failed);

        return new MassRefundResult(
            scopeId, allPairs.Count, succeeded, skipped, failed, resultItems);
    }
}