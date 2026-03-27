using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.Application.Abstractions.Locks;
using Ticketing.Application.Helpers;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;

namespace Ticketing.Application.Orders.Commands.CreateOrder;

internal sealed class CreateOrderCommandHandler(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ITicketLockService ticketLockService,
    IOrderRepository orderRepository,
    IVoucherRepository voucherRepository,
    ILogger<CreateOrderCommandHandler> logger,
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<CreateOrderCommand, Guid>
{
    private static readonly TimeSpan LockTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<Guid>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<Guid>(Error.Unauthorized("Order.Create.Unauthorized", "Current user is not authenticated."));

        var utcNow = dateTimeProvider.UtcNow;
        var seatLocks = new List<(Guid SessionId, Guid SeatId)>();
        var zoneLocks = new List<(Guid SessionId, Guid TicketTypeId, int Quantity)>();
        var isSaved = false;

        Voucher? voucher = null;
        bool isVoucherUsageIncremented = false;

        try
        {
            // Retrieve Ticketing Item information
            var pairs = command.Tickets.Select(t => (t.EventSessionId, t.TicketTypeId)).ToList();
            var itemMap = await eventTicketingPublicApi.GetTicketingItemsBatchAsync(pairs, utcNow, cancellationToken);

            if (itemMap.Count == 0)
                return Result.Failure<Guid>(TicketingErrors.Order.InvalidTicketSelection);

            // Determine Event Area Type based on the first ticket's item
            var eventAreaType = itemMap.First().Value.AreaType;

            // Validate and lock base on Event Area Type
            if (eventAreaType is EventAreaType.Seat or EventAreaType.Default)
            {
                var seatResult = await ProcessSeatTicketsAsync(command, itemMap, userId, seatLocks, cancellationToken);
                if (seatResult.IsFailure) return Result.Failure<Guid>(seatResult.Error);
            }
            else
            {
                var zoneResult = await ProcessZoneTicketsAsync(command, itemMap, zoneLocks, cancellationToken);
                if (zoneResult.IsFailure) return Result.Failure<Guid>(zoneResult.Error);
            }

            // Processing Voucher logic
            if (!string.IsNullOrWhiteSpace(command.CouponCode))
            {
                voucher = await voucherRepository.GetByCouponCodeAsync(command.CouponCode, cancellationToken);

                if (voucher is null)
                    return Result.Failure<Guid>(TicketingErrors.Voucher.NotFound(command.CouponCode));

                if (voucher.StartDate > utcNow || voucher.EndDate < utcNow)
                    return Result.Failure<Guid>(TicketingErrors.Voucher.Expired);

                if (voucher.TotalUse >= voucher.MaxUse)
                    return Result.Failure<Guid>(TicketingErrors.Voucher.ExceededMaxUse);

                var hasUsed = await voucherRepository.HasUserUsedVoucherAsync(voucher.Id, userId, cancellationToken);
                if (hasUsed)
                    return Result.Failure<Guid>(TicketingErrors.Voucher.AlreadyUsedByUser);

                // hold voucher
                voucher.IncrementUsage();
                isVoucherUsageIncremented = true;
            }

            // 4. Double-check DB committed seats
            if (seatLocks.Count > 0)
            {
                var committedSeats = await orderRepository.GetCommittedSeatsAsync(seatLocks, cancellationToken);
                var conflicted = seatLocks.FirstOrDefault(s => committedSeats.Contains((s.SessionId, s.SeatId)));
                if (conflicted != default) return Result.Failure<Guid>(TicketingErrors.Order.SeatNotAvailable);
            }

            // 5. Build Order 
            var order = Order.Create(userId, command.EventId, utcNow);
            var originalTotalPrice = 0m;

            foreach (var ticket in command.Tickets)
            {
                var item = itemMap[(ticket.EventSessionId, ticket.TicketTypeId)];
                var orderTicketId = Guid.NewGuid();
                var qrCode = QrCodeHelper.Generate(orderTicketId);

                var addResult = order.AddTicket(ticket.EventSessionId, ticket.TicketTypeId, ticket.SeatId, qrCode, item.Price, orderTicketId, utcNow);
                if (addResult.IsFailure) return Result.Failure<Guid>(addResult.Error);

                originalTotalPrice += item.Price;
            }

            var setPriceResult = order.SetOriginalTotalPrice(originalTotalPrice, utcNow);
            if (setPriceResult.IsFailure) return Result.Failure<Guid>(setPriceResult.Error);

            // 6. Apply voucher if has
            if (voucher is not null)
            {
                var discountAmount = voucher.Type switch
                {
                    VoucherType.Percentage => Math.Round(originalTotalPrice * voucher.Value / 100, 2),
                    VoucherType.Fixed => voucher.Value,
                    _ => 0m
                };

                var applyResult = order.ApplyVoucher(voucher.Id, discountAmount, utcNow);
                if (applyResult.IsFailure) return Result.Failure<Guid>(applyResult.Error);
            }

            orderRepository.Add(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            isSaved = true;
            return Result.Success(order.Id);
        }
        finally
        {
            // Rollback Locks and Voucher if order creation failed
            if (!isSaved)
            {
                try
                {
                    await ReleaseLocksAsync(seatLocks, zoneLocks);

                    if (isVoucherUsageIncremented && voucher is not null)
                    {
                        voucher.DecrementUsage();
                        await unitOfWork.SaveChangesAsync(CancellationToken.None);
                    }
                }
                catch (Exception ex) {
                    logger.LogError(ex,
                        "Failed to release locks or rollback voucher usage for User {UserId} during failed Order creation for Event {EventId}.",
                        userId, command.EventId);
                }
            }
        }
    }

    private async Task<Result> ProcessSeatTicketsAsync(
        CreateOrderCommand command,
        IReadOnlyDictionary<(Guid, Guid), EventTicketingItemDto> itemMap,
        Guid userId,
        List<(Guid, Guid)> seatLocks,
        CancellationToken cancellationToken)
    {
        var seatIds = command.Tickets.Where(t => t.SeatId.HasValue).Select(t => t.SeatId!.Value).ToList();

        var seatMap = seatIds.Count > 0
            ? await eventTicketingPublicApi.GetSeatsBatchAsync(seatIds, cancellationToken)
            : new Dictionary<Guid, EventSeatDto>();

        foreach (var ticket in command.Tickets)
        {
            if (!itemMap.TryGetValue((ticket.EventSessionId, ticket.TicketTypeId), out var item))
                return Result.Failure(TicketingErrors.Order.InvalidTicketSelection);

            if (!item.IsPurchasable)
                return Result.Failure(TicketingErrors.Order.TicketNotPurchasable);

            if (!ticket.SeatId.HasValue)
                return Result.Failure(TicketingErrors.Order.SeatRequired);

            if (!seatMap.TryGetValue(ticket.SeatId.Value, out var seat))
                return Result.Failure(TicketingErrors.Order.SeatNotFound);

            if (!item.AreaId.HasValue || seat.AreaId != item.AreaId.Value)
                return Result.Failure(TicketingErrors.Order.SeatNotBelongToArea);

            var locked = await ticketLockService.TryLockSeatAsync(ticket.EventSessionId, ticket.SeatId.Value, userId, LockTtl, cancellationToken);
            if (!locked) return Result.Failure(TicketingErrors.Order.SeatNotAvailable);

            seatLocks.Add((ticket.EventSessionId, ticket.SeatId.Value));
        }

        return Result.Success();
    }

    private async Task<Result> ProcessZoneTicketsAsync(
        CreateOrderCommand command,
        IReadOnlyDictionary<(Guid, Guid), EventTicketingItemDto> itemMap,
        List<(Guid, Guid, int)> zoneLocks,
        CancellationToken cancellationToken)
    {
        var zoneTicketsToBuy = command.Tickets.Select(t => (t.EventSessionId, t.TicketTypeId)).Distinct().ToList();

        var soldZoneCounts = await orderRepository.GetSoldZoneTicketsCountAsync(zoneTicketsToBuy, cancellationToken);

        foreach (var ticket in command.Tickets)
        {
            if (!itemMap.TryGetValue((ticket.EventSessionId, ticket.TicketTypeId), out var item))
                return Result.Failure(TicketingErrors.Order.InvalidTicketSelection);

            if (!item.IsPurchasable)
                return Result.Failure(TicketingErrors.Order.TicketNotPurchasable);

            if (ticket.SeatId.HasValue)
                return Result.Failure(TicketingErrors.Order.SeatMustBeNullForZone);

            soldZoneCounts.TryGetValue((ticket.EventSessionId, ticket.TicketTypeId), out var soldQuantity);
            var maxAllowed = item.Quantity - soldQuantity;

            var increased = await ticketLockService.TryIncreaseZoneLockAsync(ticket.EventSessionId, ticket.TicketTypeId, 1, maxAllowed, LockTtl, cancellationToken);
            if (!increased) return Result.Failure(TicketingErrors.Order.ZoneSoldOut);

            zoneLocks.Add((ticket.EventSessionId, ticket.TicketTypeId, 1));
        }

        return Result.Success();
    }

    private async Task ReleaseLocksAsync(
        IReadOnlyCollection<(Guid SessionId, Guid SeatId)> seatLocks,
        IReadOnlyCollection<(Guid SessionId, Guid TicketTypeId, int Quantity)> zoneLocks)
    {
        var tasks = new List<Task>();
        foreach (var s in seatLocks) tasks.Add(ticketLockService.UnlockSeatAsync(s.SessionId, s.SeatId));
        foreach (var z in zoneLocks) tasks.Add(ticketLockService.DecreaseZoneLockAsync(z.SessionId, z.TicketTypeId, z.Quantity));
        await Task.WhenAll(tasks);
    }
}
