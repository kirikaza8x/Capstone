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
//     ILogger<MassRefundByEventCommandHandler> logger) : ICommandHandler<MassRefundByEventCommand, MassRefundResultDto>
// {
//     private const int BatchSize = 50;

//     public async Task<Result<MassRefundResultDto>> Handle(MassRefundByEventCommand command, CancellationToken cancellationToken)
//     {
//         var transactions = (await transactionRepository
//             .GetAllCompletedByEventIdAsync(command.EventId, cancellationToken))
//             .ToList();

//         if (transactions.Count == 0)
//         {
//             logger.LogInformation("MassRefund: no completed transactions for EventId={EventId}", command.EventId);
//             return Result.Success(new MassRefundResultDto(command.EventId, 0, 0, 0, 0, []));
//         }

//         logger.LogInformation("MassRefund started: EventId={EventId}, Total={Total}", command.EventId, transactions.Count);

//         var items = new List<MassRefundItemResult>();
//         int succeeded = 0, skipped = 0, failed = 0;

//         foreach (var batch in transactions.Chunk(BatchSize))
//         {
//             // Pre-load all wallets for this batch in one query
//             var userIds = batch.Select(t => t.UserId).Distinct().ToList();
//             var wallets = (await walletRepository.GetByUserIdsAsync(userIds, cancellationToken))
//                 .ToDictionary(w => w.UserId);

//             foreach (var txn in batch)
//             {
//                 if (txn.InternalStatus == PaymentInternalStatus.Refunded)
//                 {
//                     skipped++;
//                     items.Add(new MassRefundItemResult(txn.Id, txn.UserId, txn.Amount, true, "Already refunded"));
//                     continue;
//                 }

//                 try
//                 {
//                     if (!wallets.TryGetValue(txn.UserId, out var wallet))
//                     {
//                         wallet = Wallet.Create(txn.UserId, initialBalance: 0);
//                         walletRepository.Add(wallet);
//                         wallets[txn.UserId] = wallet;
//                     }
//                     else if (wallet.Status == WalletStatus.Suspended)
//                     {
//                         wallet.ChangeStatus(WalletStatus.Active);
//                         walletRepository.Update(wallet);
//                     }

//                     var walletTxn = wallet.Credit(
//                         txn.Amount,
//                         $"Mass refund | EventId={command.EventId} | TxnId={txn.Id}"
//                     );
//                     walletTxn.MarkCompleted();
//                     walletRepository.Update(wallet);

//                     txn.MarkRefunded();
//                     transactionRepository.Update(txn);

//                     succeeded++;
//                     items.Add(new MassRefundItemResult(txn.Id, txn.UserId, txn.Amount, true, null));
//                 }
//                 catch (Exception ex)
//                 {
//                     failed++;
//                     logger.LogError(ex,
//                         "MassRefund failed for TxnId={TxnId}, UserId={UserId}", txn.Id, txn.UserId);
//                     items.Add(new MassRefundItemResult(txn.Id, txn.UserId, txn.Amount, false, ex.Message));
//                 }
//             }

//             await unitOfWork.SaveChangesAsync(cancellationToken);
//         }

//         logger.LogInformation(
//             "MassRefund complete: EventId={EventId}, Succeeded={S}, Skipped={Sk}, Failed={F}",
//             command.EventId, succeeded, skipped, failed);

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
