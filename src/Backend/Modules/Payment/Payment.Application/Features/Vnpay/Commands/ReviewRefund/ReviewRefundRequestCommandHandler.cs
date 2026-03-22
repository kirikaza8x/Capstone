using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Refunds.Commands.ReviewRefundRequest;

public class ReviewRefundRequestCommandHandler(
    IRefundRequestRepository refundRequestRepository,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<ReviewRefundRequestCommandHandler> logger)
    : ICommandHandler<ReviewRefundRequestCommand, ReviewRefundRequestResult>
{
    public async Task<Result<ReviewRefundRequestResult>> Handle(
        ReviewRefundRequestCommand command, CancellationToken cancellationToken)
    {
        // 1. Load refund request
        var refundRequest = await refundRequestRepository
            .GetByIdAsync(command.RefundRequestId, cancellationToken);

        if (refundRequest == null)
            return Result.Failure<ReviewRefundRequestResult>(
                Error.NotFound("Review.NotFound", "Refund request not found."));

        if (refundRequest.Status != RefundRequestStatus.Pending)
            return Result.Failure<ReviewRefundRequestResult>(
                Error.Conflict("Review.AlreadyReviewed",
                    $"Request is already {refundRequest.Status}."));

        // 2. Rejection — no money moves
        if (!command.Approved)
        {
            refundRequest.Reject(command.AdminId, command.ReviewerNote);
            refundRequestRepository.Update(refundRequest);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "RefundRequest rejected: Id={Id}, AdminId={AdminId}",
                refundRequest.Id, command.AdminId);

            return Result.Success(new ReviewRefundRequestResult(
                refundRequest.Id, refundRequest.Status,
                null, null, refundRequest.ReviewedAt!.Value));
        }

        // 3. Approval — load transaction with items
        var txn = await transactionRepository
            .GetByIdWithItemsAsync(refundRequest.PaymentTransactionId, cancellationToken);

        if (txn == null)
            return Result.Failure<ReviewRefundRequestResult>(
                Error.NotFound("Review.TransactionNotFound",
                    "Original transaction not found."));

        // 4. Resolve items to refund based on scope
        var itemsToRefund = refundRequest.Scope == RefundRequestScope.SingleItem
            ? txn.Items
                .Where(i => i.EventId == refundRequest.EventId
                         && i.InternalStatus == PaymentInternalStatus.Completed)
                .ToList()
            : txn.Items
                .Where(i => i.InternalStatus == PaymentInternalStatus.Completed)
                .ToList();

        if (itemsToRefund.Count == 0)
            return Result.Failure<ReviewRefundRequestResult>(
                Error.Failure("Review.NothingToRefund",
                    "No refundable items — they may have already been refunded."));

        var actualAmount = itemsToRefund.Sum(i => i.Amount);

        // 5. Resolve wallet — create if missing, reactivate if suspended
        var wallet = await ResolveWalletAsync(
            refundRequest.UserId, cancellationToken);

        // 6. Credit uses Refund method — distinct from top-up in history
        var walletTxn = wallet.Refund(
            actualAmount,
            $"Refund approved | RequestId={refundRequest.Id}");

        walletTxn.MarkCompleted();

        // 7. Mark each item refunded
        foreach (var item in itemsToRefund)
            item.MarkRefunded();

        // 8. Roll up parent if all items are now refunded
        if (txn.IsFullyRefunded())
            txn.MarkRefunded();

        // 9. Approve the request
        refundRequest.Approve(command.AdminId, command.ReviewerNote);

        // 10. Persist all in one unit
        transactionRepository.Update(txn);
        walletRepository.Update(wallet);
        refundRequestRepository.Update(refundRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "RefundRequest approved: Id={Id}, AdminId={AdminId}, Amount={Amount}",
            refundRequest.Id, command.AdminId, actualAmount);

        return Result.Success(new ReviewRefundRequestResult(
            refundRequest.Id, refundRequest.Status,
            actualAmount, wallet.Balance,
            refundRequest.ReviewedAt!.Value));
    }

    private async Task<Wallet> ResolveWalletAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
        {
            wallet = Wallet.Create(userId);
            walletRepository.Add(wallet);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return wallet;
        }

        if (wallet.Status == WalletStatus.Suspended)
        {
            wallet.ChangeStatus(WalletStatus.Active);
            walletRepository.Update(wallet);
        }

        return wallet;
    }
}