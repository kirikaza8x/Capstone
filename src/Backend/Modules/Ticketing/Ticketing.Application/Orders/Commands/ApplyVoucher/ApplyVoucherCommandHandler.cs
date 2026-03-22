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
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<ApplyVoucherCommand, ApplyVoucherResponse>
{
    public async Task<Result<ApplyVoucherResponse>> Handle(
        ApplyVoucherCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<ApplyVoucherResponse>(Error.Unauthorized(
                "ApplyVoucher.Unauthorized",
                "Current user is not authenticated."));

        // Load order
        var order = await orderRepository.GetByIdWithVouchersAsync(
            command.OrderId,
            cancellationToken);

        if (order is null)
            return Result.Failure<ApplyVoucherResponse>(
                TicketingErrors.Order.NotFound(command.OrderId));

        if (order.UserId != userId)
            return Result.Failure<ApplyVoucherResponse>(Error.Forbidden(
                "ApplyVoucher.Forbidden",
                "You are not allowed to apply voucher to this order."));

        if (order.Status != OrderStatus.Pending)
            return Result.Failure<ApplyVoucherResponse>(
                TicketingErrors.Order.NotPending);

        // Load voucher
        var utcNow = dateTimeProvider.UtcNow;

        var voucher = await voucherRepository.GetByCouponCodeAsync(
            command.CouponCode,
            cancellationToken);

        if (voucher is null)
            return Result.Failure<ApplyVoucherResponse>(
                TicketingErrors.Voucher.NotFound(command.CouponCode));

        // Validate voucher 
        if (voucher.StartDate > utcNow || voucher.EndDate < utcNow)
            return Result.Failure<ApplyVoucherResponse>(
                TicketingErrors.Voucher.Expired);

        // check same voucher
        var existingOrderVoucher = order.OrderVouchers.FirstOrDefault();
        var isSameVoucher = existingOrderVoucher?.VoucherId == voucher.Id;

        // check max use
        if (!isSameVoucher && voucher.TotalUse >= voucher.MaxUse)
            return Result.Failure<ApplyVoucherResponse>(
                TicketingErrors.Voucher.ExceededMaxUse);

        // Check user already used this voucher
        if (!isSameVoucher)
        {
            var hasUsed = await voucherRepository.HasUserUsedVoucherAsync(
                voucher.Id,
                userId,
                cancellationToken);

            if (hasUsed)
                return Result.Failure<ApplyVoucherResponse>(
                    TicketingErrors.Voucher.AlreadyUsedByUser);
        }

        // if voucher already applied to order, decrement usage of old voucher before apply new voucher
        if (existingOrderVoucher is not null && !isSameVoucher)
        {
            var oldVoucher = await voucherRepository.GetByIdAsync(
                existingOrderVoucher.VoucherId,
                cancellationToken);

            oldVoucher?.DecrementUsage();
        }

        // calculate discount amount
        var originalPrice = order.TotalPrice;
        if (isSameVoucher)
            originalPrice += existingOrderVoucher!.DiscountAmount;

        var discountAmount = voucher.Type switch
        {
            VoucherType.Percentage => Math.Round(originalPrice * voucher.Value / 100, 2),
            VoucherType.Fixed => voucher.Value,
            _ => 0m
        };

        // apply
        var applyResult = order.ApplyVoucher(
            voucher.Id,
            discountAmount,
            utcNow);

        if (applyResult.IsFailure)
            return Result.Failure<ApplyVoucherResponse>(applyResult.Error);

        // increate total use of new voucher if it's not same voucher
        if (!isSameVoucher)
            voucher.IncrementUsage();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApplyVoucherResponse(
            order.Id,
            originalPrice,
            discountAmount,
            order.TotalPrice));
    }
}
