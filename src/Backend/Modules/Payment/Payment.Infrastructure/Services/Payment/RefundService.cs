// // Infrastructure/Services/MassRefundProcessor.cs
// using Microsoft.Extensions.Logging; //  Fix CS0246
// using Payment.Domain.Enums;
// using Payments.Domain.Entities;
// using Payments.Domain.Enums;
// using Payments.Domain.Repositories;
// using Payments.Domain.UOW;

// public class MassRefundProcessor : IMassRefundProcessor
// {
//     private readonly IWalletRepository _walletRepository;
//     private readonly IPaymentTransactionRepository _transactionRepository; //  NEW: to fetch parent txn
//     private readonly IPaymentUnitOfWork _unitOfWork;
//     private readonly ILogger<MassRefundProcessor> _logger;

//     public MassRefundProcessor(
//         IWalletRepository walletRepository,
//         IPaymentTransactionRepository transactionRepository, //  Inject
//         IPaymentUnitOfWork unitOfWork,
//         ILogger<MassRefundProcessor> logger)
//     {
//         _walletRepository = walletRepository;
//         _transactionRepository = transactionRepository;
//         _unitOfWork = unitOfWork;
//         _logger = logger;
//     }

//     public async Task<MassRefundResult> ProcessAsync(
//         IEnumerable<BatchPaymentItem> itemsToRefund,
//         string refundReason,
//         CancellationToken ct)
//     {
//         var errors = new List<string>();
//         var succeeded = 0;
//         var skipped = 0;
//         var failed = 0;

//         var txnIds = itemsToRefund.Select(x => x.PaymentTransactionId).Distinct();
//         var transactions = await _transactionRepository.GetByIdsAsync(txnIds, ct);
//         var txnLookup = transactions.ToDictionary(t => t.Id);

//         //  Group items by UserId (via parent transaction)
//         var groupedByUser = itemsToRefund
//             .Where(item => txnLookup.ContainsKey(item.PaymentTransactionId))
//             .GroupBy(item => txnLookup[item.PaymentTransactionId].UserId);

//         foreach (var userGroup in groupedByUser)
//         {
//             try
//             {
//                 var userId = userGroup.Key;
                
//                 // 1. Get or Create Wallet
//                 var wallet = await _walletRepository.GetByIdAsync(userId, ct);
//                 if (wallet == null)
//                 {
//                     wallet = Wallet.Create(userId);
//                     await _walletRepository.Add(wallet); 
//                 }
//                 else if (wallet.Status == WalletStatus.Suspended)
//                 {
//                     wallet.ChangeStatus(WalletStatus.Active); 
//                 }

//                 // 2. Calculate total refund amount for this user
//                 var totalRefundAmount = userGroup.Sum(x => x.Amount);

//                 // 3. Credit wallet using your dedicated Refund() method 
//                 wallet.Refund(totalRefundAmount, refundReason);
                
//                 // 4. Mark each item as refunded
//                 foreach (var item in userGroup)
//                 {
//                     // Double-check eligibility (idempotency)
//                     if (item.RefundedAt.HasValue || 
//                         item.InternalStatus == PaymentInternalStatus.Refunded)
//                     {
//                         skipped++;
//                         continue;
//                     }

//                     item.MarkRefunded(); 
//                     succeeded++;
//                 }

//                 _logger.LogInformation(
//                     "Refunded {Amount} to User {UserId} for {Count} items", 
//                     totalRefundAmount, userId, userGroup.Count());
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Failed to refund user {UserId}", userGroup.Key);
//                 failed += userGroup.Count();
//                 errors.Add($"User {userGroup.Key}: {ex.Message}");
//                 // Continue processing other users (partial success)
//             }
//         }

//         return new MassRefundResult(
//             TotalProcessed: itemsToRefund.Count(),
//             TotalSucceeded: succeeded,
//             TotalSkipped: skipped,
//             TotalFailed: failed,
//             Errors: errors
//         );
//     }
// }