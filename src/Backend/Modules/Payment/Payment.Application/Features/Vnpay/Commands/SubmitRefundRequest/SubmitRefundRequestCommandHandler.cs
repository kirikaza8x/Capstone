using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.DTOs.Refund;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Refunds.Commands.SubmitRefundRequest;

public class SubmitRefundRequestCommandHandler(
    IPaymentTransactionRepository transactionRepository,
    IRefundRequestRepository refundRequestRepository,
    ICurrentUserService currentUserService,
    IPaymentUnitOfWork unitOfWork,
    ILogger<SubmitRefundRequestCommandHandler> logger)
    : ICommandHandler<SubmitRefundRequestCommand, RefundRequestDto>
{
    public async Task<Result<RefundRequestDto>> Handle(
        SubmitRefundRequestCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        // 1. Load transaction with items
        var txn = await transactionRepository
            .GetByIdWithItemsAsync(
                command.PaymentTransactionId, cancellationToken);

        if (txn == null)
            return Result.Failure<RefundRequestDto>(
                Error.NotFound("Refund.TransactionNotFound",
                    "Payment transaction not found."));

        // 2. Ownership check
        if (txn.UserId != userId)
            return Result.Failure<RefundRequestDto>(
                Error.Forbidden("Refund.Forbidden",
                    "You do not have access to this transaction."));

        // 3. Must be completed
        if (txn.InternalStatus != PaymentInternalStatus.Completed)
            return Result.Failure<RefundRequestDto>(
                Error.Failure("Refund.NotCompleted",
                    "Only completed payments can be refunded."));

        // 4. Resolve scope and amount
        decimal requestedAmount;
        Guid? eventSessionId = null;

        if (command.Scope == RefundRequestScope.SingleItem)
        {
            if (!command.EventSessionId.HasValue)
                return Result.Failure<RefundRequestDto>(
                    Error.Validation("Refund.MissingEventSessionId",
                        "EventSessionId is required for single item refund."));

            var item = txn.Items.FirstOrDefault(i =>
                i.EventSessionId == command.EventSessionId.Value &&
                i.InternalStatus == PaymentInternalStatus.Completed);

            if (item == null)
                return Result.Failure<RefundRequestDto>(
                    Error.NotFound("Refund.ItemNotFound",
                        "No refundable item found for this session."));

            requestedAmount = item.Amount;
            eventSessionId = item.EventSessionId;
        }
        else
        {
            var refundableItems = txn.Items
                .Where(i => i.InternalStatus == PaymentInternalStatus.Completed)
                .ToList();

            if (refundableItems.Count == 0)
                return Result.Failure<RefundRequestDto>(
                    Error.Failure("Refund.NothingToRefund",
                        "All items have already been refunded."));

            requestedAmount = refundableItems.Sum(i => i.Amount);
        }

        // 5. Duplicate pending guard
        var hasPending = await refundRequestRepository
            .HasPendingRequestAsync(
                txn.Id, eventSessionId, cancellationToken);

        if (hasPending)
            return Result.Failure<RefundRequestDto>(
                Error.Conflict("Refund.DuplicatePending",
                    "A pending refund request already exists for this item."));

        // 6. Create — no money moves yet
        var refundRequest = command.Scope == RefundRequestScope.SingleItem
            ? RefundRequest.CreateSingleItem(
                userId, txn.Id, eventSessionId!.Value,
                requestedAmount, command.UserReason)
            : RefundRequest.CreateFullBatch(
                userId, txn.Id,
                requestedAmount, command.UserReason);

        refundRequestRepository.Add(refundRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "RefundRequest submitted: Id={Id}, UserId={UserId}, " +
            "Scope={Scope}, Amount={Amount}",
            refundRequest.Id, userId,
            command.Scope, requestedAmount);

        return Result.Success(MapToDto(refundRequest));
    }

    private static RefundRequestDto MapToDto(RefundRequest r) => new(
        Id: r.Id,
        UserId: r.UserId,
        PaymentTransactionId: r.PaymentTransactionId,
        EventSessionId: r.EventSessionId,
        Scope: r.Scope,
        Status: r.Status,
        RequestedAmount: r.RequestedAmount,
        UserReason: r.UserReason,
        ReviewerNote: r.ReviewerNote,
        ReviewedByAdminId: r.ReviewedByAdminId,
        ReviewedAt: r.ReviewedAt,
        CreatedAt: r.CreatedAt);
}
