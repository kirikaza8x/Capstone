using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;

namespace Ticketing.Application.Orders.Commands.ApplyVoucher;

internal sealed class ApplyVoucherCommandHandler(
    IOrderRepository orderRepository,
    IVoucherRepository voucherRepository,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    ILogger<ApplyVoucherCommandHandler> logger,
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<ApplyVoucherCommand, ApplyVoucherResponse>
{
    public async Task<Result<ApplyVoucherResponse>> Handle(
        ApplyVoucherCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<ApplyVoucherResponse>(Error.Unauthorized("ApplyVoucher.Unauthorized", "Current user is not authenticated."));

        // Load Order 
        var order = await orderRepository.GetByIdWithVouchersAsync(command.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure<ApplyVoucherResponse>(TicketingErrors.Order.NotFound(command.OrderId));

        if (order.UserId != userId)
            return Result.Failure<ApplyVoucherResponse>(Error.Forbidden("ApplyVoucher.Forbidden", "Not allowed to modify this order."));

        if (order.Status != OrderStatus.Pending)
            return Result.Failure<ApplyVoucherResponse>(TicketingErrors.Order.NotPending);

        // Load New Voucher
        var utcNow = dateTimeProvider.UtcNow;
        var newVoucher = await voucherRepository.GetByCouponCodeAsync(command.CouponCode, cancellationToken);

        if (newVoucher is null)
            return Result.Failure<ApplyVoucherResponse>(TicketingErrors.Voucher.NotFound(command.CouponCode));

        // Ensure the voucher belongs to the same event
        if (newVoucher.EventId.HasValue && newVoucher.EventId != order.EventId)
            return Result.Failure<ApplyVoucherResponse>(TicketingErrors.Voucher.InvalidEvent);

        // Validate New Voucher
        if (newVoucher.StartDate > utcNow || newVoucher.EndDate < utcNow)
            return Result.Failure<ApplyVoucherResponse>(TicketingErrors.Voucher.Expired);

        var existingOrderVoucher = order.OrderVouchers.FirstOrDefault();
        var isSameVoucher = existingOrderVoucher?.VoucherId == newVoucher.Id;

        // Idempotency: If the user clicks apply multiple times with the same code, return success immediately
        if (isSameVoucher)
        {
            return Result.Success(new ApplyVoucherResponse(
                order.Id, order.OriginalTotalPrice, existingOrderVoucher!.DiscountAmount, order.TotalPrice));
        }

        if (newVoucher.TotalUse >= newVoucher.MaxUse)
            return Result.Failure<ApplyVoucherResponse>(TicketingErrors.Voucher.ExceededMaxUse);

        var hasUsed = await voucherRepository.HasUserUsedVoucherAsync(newVoucher.Id, userId, cancellationToken);
        if (hasUsed)
            return Result.Failure<ApplyVoucherResponse>(TicketingErrors.Voucher.AlreadyUsedByUser);

        // Release the old voucher if the order already has one
        if (existingOrderVoucher is not null)
        {
            var oldVoucher = await voucherRepository.GetByIdAsync(existingOrderVoucher.VoucherId, cancellationToken);
            if (oldVoucher is not null)
            {
                oldVoucher.DecrementUsage();
                logger.LogInformation("Released old voucher {OldVoucherCode} from Order {OrderId}", oldVoucher.CouponCode, order.Id);
            }

            order.RemoveVoucher();
        }

        // Calculate Discount
        var originalPrice = order.OriginalTotalPrice;
        var discountAmount = newVoucher.Type switch
        {
            VoucherType.Percentage => Math.Round(originalPrice * newVoucher.Value / 100, 2),
            VoucherType.Fixed => newVoucher.Value > originalPrice ? originalPrice : newVoucher.Value,
            _ => 0m
        };

        // Apply to Order and Hold new Voucher
        var applyResult = order.ApplyVoucher(newVoucher.Id, discountAmount, utcNow);
        if (applyResult.IsFailure)
            return Result.Failure<ApplyVoucherResponse>(applyResult.Error);

        newVoucher.IncrementUsage();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Successfully applied voucher {CouponCode} to Order {OrderId}. Discount: {DiscountAmount}",
            command.CouponCode, order.Id, discountAmount);

        return Result.Success(new ApplyVoucherResponse(
            order.Id,
            originalPrice,
            discountAmount,
            order.TotalPrice));
    }
}
