using Microsoft;
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
            logger.LogInformation("MassRefund: no items found for ScopeId={ScopeId}", scopeId);
            return new MassRefundResult(scopeId, 0, 0, 0, 0, []);
        }

        logger.LogInformation("MassRefund started: ScopeId={ScopeId}, Total={Total}", scopeId, allPairs.Count);

        var resultItems = new List<MassRefundItemResult>();
        int succeeded = 0, skipped = 0, failed = 0;

        foreach (var batch in allPairs.Chunk(BatchSize))
        {
            var userIds = batch.Select(p => p.Transaction.UserId).Distinct().ToList();
            var walletMap = (await walletRepository.GetByUserIdsAsync(userIds, ct))
                .ToDictionary(w => w.UserId);

            var missingIds = userIds.Except(walletMap.Keys).ToList();
            if (missingIds.Count > 0)
            {
                var newWallets = missingIds.Select(uid => Wallet.Create(uid)).ToList();
                await walletRepository.BulkInsertAsync(newWallets, ct);

                foreach (var w in newWallets)
                    walletMap[w.UserId] = w;

                logger.LogInformation("MassRefund: created {Count} new wallets for ScopeId={ScopeId}", newWallets.Count, scopeId);
            }

            var txnsToUpdate = new List<PaymentTransaction>();
            var walletsToUpdate = new List<Wallet>();

            foreach (var (txn, item) in batch)
            {
                if (item.InternalStatus == PaymentInternalStatus.Refunded)
                {
                    skipped++;
                    resultItems.Add(new MassRefundItemResult(txn.Id, txn.UserId, item.Amount, true, "Already refunded"));
                    logger.LogDebug("MassRefund skipped: TxnId={TxnId}, UserId={UserId}, Amount={Amount}", txn.Id, txn.UserId, item.Amount);
                    continue;
                }

                try
                {
                    var wallet = walletMap[txn.UserId];

                    if (wallet.Status == WalletStatus.Suspended)
                    {
                        wallet.ChangeStatus(WalletStatus.Active);
                        logger.LogWarning("MassRefund: reactivated suspended wallet for UserId={UserId}", txn.UserId);
                    }

                    var walletTxn = wallet.Refund(item.Amount, $"{refundNotePrefix} | TxnId={txn.Id}");
                    walletTxn.MarkCompleted();

                    txn.MarkItemRefunded(item, txn.UserId);

                    txnsToUpdate.Add(txn);
                    walletsToUpdate.Add(wallet);

                    succeeded++;
                    resultItems.Add(new MassRefundItemResult(txn.Id, txn.UserId, item.Amount, true, null));
                    logger.LogDebug("MassRefund succeeded: TxnId={TxnId}, UserId={UserId}, Amount={Amount}", txn.Id, txn.UserId, item.Amount);
                }
                catch (Exception ex)
                {
                    failed++;
                    logger.LogError(ex, "MassRefund item failed: TxnId={TxnId}, UserId={UserId}, Amount={Amount}", txn.Id, txn.UserId, item.Amount);

                    resultItems.Add(new MassRefundItemResult(txn.Id, txn.UserId, item.Amount, false, ex.Message));
                }
            }

            if (txnsToUpdate.Count == 0) continue;

            try
            {
                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    transactionRepository.UpdateRange(txnsToUpdate);
                    walletRepository.UpdateRange(walletsToUpdate);

                    logger.LogInformation("MassRefund batch committed: ScopeId={ScopeId}, BatchSize={BatchSize}, Succeeded={Succeeded}, Skipped={Skipped}, Failed={Failed}",
                        scopeId, batch.Count(), succeeded, skipped, failed);
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MassRefund batch rolled back: ScopeId={ScopeId}, Txns={Txns}, Users={Users}",
                    scopeId,
                    string.Join(",", txnsToUpdate.Select(t => t.Id)),
                    string.Join(",", walletsToUpdate.Select(w => w.UserId)));

                foreach (var txn in txnsToUpdate)
                {
                    var idx = resultItems.FindLastIndex(i => i.PaymentTransactionId == txn.Id && i.Success);
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

        logger.LogInformation("MassRefund complete: ScopeId={ScopeId}, Total={Total}, Succeeded={Succeeded}, Skipped={Skipped}, Failed={Failed}",
            scopeId, allPairs.Count, succeeded, skipped, failed);

        return new MassRefundResult(scopeId, allPairs.Count, succeeded, skipped, failed, resultItems);
    }
}
