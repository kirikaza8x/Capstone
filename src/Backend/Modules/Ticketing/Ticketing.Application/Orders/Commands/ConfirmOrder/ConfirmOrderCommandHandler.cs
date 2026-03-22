using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Application.Abstractions.Locks;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;

namespace Ticketing.Application.Orders.Commands.ConfirmOrder;

internal sealed class ConfirmOrderCommandHandler(
    IOrderRepository orderRepository,
    ITicketLockService ticketLockService,
    ITicketingUnitOfWork unitOfWork,
    ILogger<ConfirmOrderCommandHandler> logger) : ICommandHandler<ConfirmOrderCommand>
{
    public async Task<Result> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithOrderTicketAsync(
            command.OrderId,
            cancellationToken);

        if (order is null)
            return Result.Failure(TicketingErrors.Order.NotFound(command.OrderId));

        if (order.Status == OrderStatus.Paid)
            return Result.Success();

        if (order.Status == OrderStatus.Cancelled)
        {
            logger.LogWarning(
                "Order {OrderId} is already cancelled, skipping payment confirmation.",
                command.OrderId);
            return Result.Success();
        }

        // mark order as paid
        var result = order.MarkPaid(command.PaidAtUtc);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // release redis locks
        try
        {
            var seatTasks = order.Tickets
                .Where(t => t.SeatId.HasValue)
                .Select(t => ticketLockService.UnlockSeatAsync(
                    t.EventSessionId,
                    t.SeatId!.Value,
                    cancellationToken));

            var zoneTasks = order.Tickets
                .Where(t => !t.SeatId.HasValue)
                .GroupBy(t => new { t.EventSessionId, t.TicketTypeId })
                .Select(g => ticketLockService.DecreaseZoneLockAsync(
                    g.Key.EventSessionId,
                    g.Key.TicketTypeId,
                    g.Count(),
                    cancellationToken));

            await Task.WhenAll(seatTasks.Concat(zoneTasks));
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Order {OrderId} marked as paid but failed to release some redis locks.",
                order.Id);
        }

        return Result.Success();
    }
}
