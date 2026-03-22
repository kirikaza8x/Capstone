using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.DTOs.Refund;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Refunds.Commands.SubmitRefundRequest;

public class SubmitRefundRequestCommandHandler(
    ICurrentUserService currentUser,
    IPaymentTransactionRepository transactionRepository,
    IRefundRequestRepository refundRequestRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<SubmitRefundRequestCommandHandler> logger)
    : ICommandHandler<SubmitRefundRequestCommand, RefundRequestDto>
{
    public async Task<Result<RefundRequestDto>> Handle(
        SubmitRefundRequestCommand command, CancellationToken cancellationToken)
    {
        // 1. Load transaction with items
        var txn = await transactionRepository
            .GetByIdWithItemsAsync(command.PaymentTransactionId, cancellationToken);

        if (txn == null)
            return Result.Failure<RefundRequestDto>(
                Error.NotFound("Refund.TransactionNotFound",
                    "Payment transaction not found."));

        // 2. Must belong to requesting user
        if (txn.UserId != currentUser.UserId)
            return Result.Failure<RefundRequestDto>(
                Error.Forbidden("Refund.Forbidden",
                    "You do not have access to this transaction."));

        // 3. Must be completed
        if (txn.InternalStatus != PaymentInternalStatus.Completed)
            return Result.Failure<RefundRequestDto>(
                Error.Failure("Refund.NotCompleted",
                    "Only completed payments can be refunded."));

        // 4. Resolve scope + amount
        decimal requestedAmount;
        Guid? eventId = null;

        if (command.Scope == RefundRequestScope.SingleItem)
        {
            if (!command.EventId.HasValue)
                return Result.Failure<RefundRequestDto>(
                    Error.Validation("Refund.MissingEventId",
                        "EventId is required for single item refund."));

            var item = txn.Items.FirstOrDefault(i =>
                i.EventId == command.EventId.Value &&
                i.InternalStatus == PaymentInternalStatus.Completed);

            if (item == null)
                return Result.Failure<RefundRequestDto>(
                    Error.NotFound("Refund.ItemNotFound",
                        "No refundable item found for this event."));

            requestedAmount = item.Amount;
            eventId = item.EventId;
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
            .HasPendingRequestAsync(txn.Id, eventId, cancellationToken);

        if (hasPending)
            return Result.Failure<RefundRequestDto>(
                Error.Conflict("Refund.DuplicatePending",
                    "A pending refund request already exists for this item."));

        // 6. Create — no money moves yet
        var refundRequest = command.Scope == RefundRequestScope.SingleItem
            ? RefundRequest.CreateSingleItem(
                currentUser.UserId, txn.Id, eventId!.Value,
                requestedAmount, command.UserReason)
            : RefundRequest.CreateFullBatch(
                currentUser.UserId, txn.Id,
                requestedAmount, command.UserReason);

        refundRequestRepository.Add(refundRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "RefundRequest submitted: Id={Id}, UserId={UserId}, Scope={Scope}, Amount={Amount}",
            refundRequest.Id, currentUser.UserId, command.Scope, requestedAmount);

        return Result.Success(MapToDto(refundRequest));
    }

    private static RefundRequestDto MapToDto(RefundRequest r) => new(
        r.Id, r.UserId, r.PaymentTransactionId, r.EventId,
        r.Scope, r.Status, r.RequestedAmount, r.UserReason,
        r.ReviewerNote, r.ReviewedByAdminId, r.ReviewedAt, r.CreatedAt);
}