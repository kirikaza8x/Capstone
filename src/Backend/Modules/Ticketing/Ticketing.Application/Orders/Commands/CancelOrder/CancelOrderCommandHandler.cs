using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.Application.Abstractions.Locks;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;

namespace Ticketing.Application.Orders.Commands.CancelOrder;

internal sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IVoucherRepository voucherRepository,
    ITicketLockService ticketLockService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    ITicketingUnitOfWork unitOfWork,
    ILogger<CancelOrderCommandHandler> logger) : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(
        CancelOrderCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure(Error.Unauthorized(
                "CancelOrder.Unauthorized",
                "Current user is not authenticated."));

        // Load Order 
        var order = await orderRepository.GetByIdWithOrderTicketAsync(
            command.OrderId,
            cancellationToken);

        if (order is null)
            return Result.Failure(TicketingErrors.Order.NotFound(command.OrderId));

        if (order.UserId != userId)
            return Result.Failure(Error.Forbidden(
                "CancelOrder.Forbidden",
                "You are not allowed to cancel this order."));

        // Cancel aggregate
        var utcNow = dateTimeProvider.UtcNow;
        var cancelResult = order.Cancel(utcNow);
        if (cancelResult.IsFailure)
            return cancelResult;

        // Decrease total_use voucher
        var existingVoucher = order.OrderVouchers.FirstOrDefault();
        if (existingVoucher is not null)
        {
            var voucher = await voucherRepository.GetByIdAsync(
                existingVoucher.VoucherId,
                cancellationToken);

            voucher?.DecrementUsage();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Release Redis locks
        try
        {
            var seatTasks = order.Tickets
                .Where(t => t.SeatId.HasValue)
                .Select(t => ticketLockService.UnlockSeatAsync(
                    t.EventSessionId,
                    t.SeatId!.Value,
                    CancellationToken.None));

            var zoneTasks = order.Tickets
                .Where(t => !t.SeatId.HasValue)
                .GroupBy(t => new { t.EventSessionId, t.TicketTypeId })
                .Select(g => ticketLockService.DecreaseZoneLockAsync(
                    g.Key.EventSessionId,
                    g.Key.TicketTypeId,
                    g.Count(),
                    CancellationToken.None));

            await Task.WhenAll(seatTasks.Concat(zoneTasks));
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Order {OrderId} cancelled but failed to release some redis locks.",
                order.Id);
        }

        return Result.Success();
    }
}
