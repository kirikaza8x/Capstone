using Microsoft.Extensions.Logging;
using Quartz;
using Ticketing.Application.Abstractions.Locks;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;

namespace Ticketing.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class CancelExpiredPendingOrdersJob(
    IOrderRepository orderRepository,
    IVoucherRepository voucherRepository,
    ITicketLockService ticketLockService,
    ITicketingUnitOfWork unitOfWork,
    ILogger<CancelExpiredPendingOrdersJob> logger) : IJob
{
    private const int BatchSize = 100;
    private static readonly TimeSpan PendingTtl = TimeSpan.FromMinutes(10);

    public async Task Execute(IJobExecutionContext context)
    {
        var utcNow = DateTime.UtcNow;
        var expiredBeforeUtc = utcNow.Subtract(PendingTtl);

        var orders = await orderRepository.GetPendingExpiredWithTicketsAsync(
            expiredBeforeUtc,
            BatchSize,
            context.CancellationToken);

        if (orders.Count == 0)
            return;

        var cancelledCount = 0;

        foreach (var order in orders)
        {
            var cancelResult = order.Cancel(utcNow);
            if (cancelResult.IsFailure)
                continue;

            cancelledCount++;

            // Rollback the number of voucher uses if this order has the code applied.
            var orderVoucher = order.OrderVouchers.FirstOrDefault();
            if (orderVoucher is not null)
            {
                var voucher = await voucherRepository.GetByIdAsync(orderVoucher.VoucherId, context.CancellationToken);
                if (voucher is not null)
                {
                    voucher.DecrementUsage();
                }
            }

            // release seat locks
            var seatTasks = order.Tickets
                .Where(t => t.SeatId.HasValue)
                .Select(t => ticketLockService.UnlockSeatAsync(
                    t.EventSessionId,
                    t.SeatId!.Value,
                    context.CancellationToken));

            // release zone locks by grouped quantity
            var zoneTasks = order.Tickets
                .Where(t => !t.SeatId.HasValue)
                .GroupBy(t => new { t.EventSessionId, t.TicketTypeId })
                .Select(g => ticketLockService.DecreaseZoneLockAsync(
                    g.Key.EventSessionId,
                    g.Key.TicketTypeId,
                    g.Count(),
                    context.CancellationToken));

            await Task.WhenAll(seatTasks.Concat(zoneTasks));
        }

        if (cancelledCount > 0)
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "CancelExpiredPendingOrdersJob processed {Total} order(s), cancelled {Cancelled}.",
            orders.Count,
            cancelledCount);
    }
}
