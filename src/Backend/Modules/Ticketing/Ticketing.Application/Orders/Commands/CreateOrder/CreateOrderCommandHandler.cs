using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.Application.Abstractions.Locks;
using Ticketing.Application.Helpers;
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
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<CreateOrderCommand, Guid>
{
    private static readonly TimeSpan LockTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<Guid>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<Guid>(Error.Unauthorized(
                "Order.Create.Unauthorized",
                "Current user is not authenticated."));

        var utcNow = dateTimeProvider.UtcNow;

        // Track locks to release if anything fails before saving
        var seatLocks = new List<(Guid SessionId, Guid SeatId)>();
        var zoneLocks = new List<(Guid SessionId, Guid TicketTypeId, int Quantity)>();
        var isSaved = false;

        try
        {
            // Batch fetch all from event public API
            var pairs = command.Tickets
                .Select(t => (t.EventSessionId, t.TicketTypeId))
                .ToList();

            var itemMap = await eventTicketingPublicApi
                .GetTicketingItemsBatchAsync(pairs, utcNow, cancellationToken);

            // Batch fetch seats if has
            var seatIds = command.Tickets
                .Where(t => t.SeatId.HasValue)
                .Select(t => t.SeatId!.Value)
                .ToList();

            var seatMap = seatIds.Count > 0
                ? await eventTicketingPublicApi.GetSeatsBatchAsync(seatIds, cancellationToken)
                : new Dictionary<Guid, EventSeatDto>();

            // validate tickets
            foreach (var ticket in command.Tickets)
            {
                if (!itemMap.TryGetValue((ticket.EventSessionId, ticket.TicketTypeId), out var item))
                    return Result.Failure<Guid>(TicketingErrors.Order.InvalidTicketSelection);

                if (!item.IsPurchasable)
                    return Result.Failure<Guid>(TicketingErrors.Order.TicketNotPurchasable);

                if (item.AreaType == EventAreaType.Zone && ticket.SeatId.HasValue)
                    return Result.Failure<Guid>(TicketingErrors.Order.SeatMustBeNullForZone);

                if (item.AreaType == EventAreaType.Seat)
                {
                    if (!ticket.SeatId.HasValue)
                        return Result.Failure<Guid>(TicketingErrors.Order.SeatRequired);

                    if (!seatMap.TryGetValue(ticket.SeatId.Value, out var seat))
                        return Result.Failure<Guid>(TicketingErrors.Order.SeatNotFound);

                    // validate seat belongs to area
                    if (!item.AreaId.HasValue || seat.AreaId != item.AreaId.Value)
                        return Result.Failure<Guid>(TicketingErrors.Order.SeatNotBelongToArea);
                }
            }

            var zoneTicketsToBuy = command.Tickets
                .Where(t => itemMap.TryGetValue((t.EventSessionId, t.TicketTypeId), out var item) && item.AreaType == EventAreaType.Zone)
                .ToList();

            // Khởi tạo dictionary chứa số lượng vé đã bán cho mỗi cặp (SessionId, TicketTypeId)
            var soldZoneCounts = new Dictionary<(Guid SessionId, Guid TicketTypeId), int>();

            if (zoneTicketsToBuy.Count > 0)
            {
                // Gọi repository để đếm số lượng vé Zone đã bán (Status = Valid/Used)
                // Bạn cần implement hàm này trong IOrderRepository
                soldZoneCounts = await orderRepository.GetSoldZoneTicketsCountAsync(
                    zoneTicketsToBuy.Select(t => (t.EventSessionId, t.TicketTypeId)).Distinct().ToList(),
                    cancellationToken);
            }

            // Acquire Redis locks 
            foreach (var ticket in command.Tickets)
            {
                var item = itemMap[(ticket.EventSessionId, ticket.TicketTypeId)];

                if (item.AreaType is EventAreaType.Seat or EventAreaType.Default)
                {
                    var locked = await ticketLockService.TryLockSeatAsync(
                        ticket.EventSessionId,
                        ticket.SeatId!.Value,
                        userId,
                        LockTtl,
                        cancellationToken);

                    if (!locked)
                        return Result.Failure<Guid>(TicketingErrors.Order.SeatNotAvailable);

                    seatLocks.Add((ticket.EventSessionId, ticket.SeatId.Value));
                }
                else
                {
                    soldZoneCounts.TryGetValue((ticket.EventSessionId, ticket.TicketTypeId), out var soldQuantity);
                    // calculate max allowed to lock based on sold quantity, if soldQuantity is not found in dictionary, it means 0 tickets sold
                    var maxAllowed = item.Quantity - soldQuantity;

                    var increased = await ticketLockService.TryIncreaseZoneLockAsync(
                        ticket.EventSessionId,
                        ticket.TicketTypeId,
                        increaseBy: 1,
                        maxAllowed: maxAllowed,
                        ttl: LockTtl,
                        cancellationToken);

                    if (!increased)
                        return Result.Failure<Guid>(TicketingErrors.Order.ZoneSoldOut);

                    zoneLocks.Add((ticket.EventSessionId, ticket.TicketTypeId, 1));
                }
            }

            // Double-check committed seats from DB
            if (seatLocks.Count > 0)
            {
                var committedSeats = await orderRepository.GetCommittedSeatsAsync(
                    seatLocks,
                    cancellationToken);

                var conflicted = seatLocks.FirstOrDefault(
                    s => committedSeats.Contains((s.SessionId, s.SeatId)));

                if (conflicted != default)
                    return Result.Failure<Guid>(TicketingErrors.Order.SeatNotAvailable);
            }

            // Build order
            var order = Domain.Entities.Order.Create(userId, command.EventId, utcNow);
            var originalTotalPrice = 0m;

            foreach (var ticket in command.Tickets)
            {
                var item = itemMap[(ticket.EventSessionId, ticket.TicketTypeId)];

                // build QR code
                var orderTicketId = Guid.NewGuid();
                var qrCode = QrCodeHelper.Generate(orderTicketId);

                var addResult = order.AddTicket(
                    ticket.EventSessionId,
                    ticket.TicketTypeId,
                    ticket.SeatId,
                    qrCode,
                    item.Price,
                    orderTicketId,
                    utcNow);

                if (addResult.IsFailure)
                    return Result.Failure<Guid>(addResult.Error);

                originalTotalPrice += item.Price;
            }

            var setPriceResult = order.SetOriginalTotalPrice(originalTotalPrice, utcNow);
            if (setPriceResult.IsFailure)
                return Result.Failure<Guid>(setPriceResult.Error);

            orderRepository.Add(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            isSaved = true;
            return Result.Success(order.Id);
        }
        finally
        {
            // Release locks if not saved
            if (!isSaved)
            {
                try
                {
                    await ReleaseLocksAsync(seatLocks, zoneLocks);
                }
                catch (Exception ex)
                {
                    _ = ex;
                }
            }
        }
    }

    private async Task ReleaseLocksAsync(
        IReadOnlyCollection<(Guid SessionId, Guid SeatId)> seatLocks,
        IReadOnlyCollection<(Guid SessionId, Guid TicketTypeId, int Quantity)> zoneLocks)
    {
        var tasks = new List<Task>();

        foreach (var s in seatLocks)
            tasks.Add(ticketLockService.UnlockSeatAsync(s.SessionId, s.SeatId));

        foreach (var z in zoneLocks)
            tasks.Add(ticketLockService.DecreaseZoneLockAsync(
                z.SessionId, z.TicketTypeId, z.Quantity));

        await Task.WhenAll(tasks);
    }
}
