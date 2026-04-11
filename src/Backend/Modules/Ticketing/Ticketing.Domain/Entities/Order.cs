using Shared.Domain.Abstractions;
using Shared.Domain.DDD;
using Ticketing.Domain.DomainEvents;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;

namespace Ticketing.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public decimal TotalPrice { get; private set; }
    public decimal OriginalTotalPrice { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderTicket> _tickets = [];
    public IReadOnlyCollection<OrderTicket> Tickets => _tickets.AsReadOnly();

    private readonly List<OrderVoucher> _orderVouchers = [];
    public IReadOnlyCollection<OrderVoucher> OrderVouchers => _orderVouchers.AsReadOnly();

    private Order() { }

    public static Order Create(Guid userId, Guid eventId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;

        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = eventId,
            Status = OrderStatus.Pending,
            TotalPrice = 0,
            OriginalTotalPrice = 0, 
            CreatedAt = now
        };
    }

    public Result SetOriginalTotalPrice(decimal originTotalPrice, DateTime? utcNow = null)
    {
        if (originTotalPrice < 0)
            return Result.Failure(TicketingErrors.Order.InvalidTotalPrice);

        OriginalTotalPrice = originTotalPrice;
        TotalPrice = originTotalPrice;
        ModifiedAt = utcNow ?? DateTime.UtcNow;
        return Result.Success();
    }

    public Result AddTicket(
        Guid eventSessionId,
        Guid ticketTypeId,
        Guid? seatId,
        string qrCode,
        decimal price,
        Guid? orderTicketId = null,
        DateTime? utcNow = null)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(TicketingErrors.Order.CannotMarkPaid(Status));

        var ticketResult = OrderTicket.Create(
            Id,
            eventSessionId,
            ticketTypeId,
            seatId,
            qrCode,
            price,
            orderTicketId,
            utcNow);

        if (ticketResult.IsFailure)
            return ticketResult;

        _tickets.Add(ticketResult.Value);
        ModifiedAt = utcNow ?? DateTime.UtcNow;
        return Result.Success();
    }

    public Result ApplyVoucher(
            Guid voucherId,
            decimal discountAmount,
            DateTime appliedAtUtc)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(TicketingErrors.Order.NotPending);

        if (discountAmount < 0)
            return Result.Failure(TicketingErrors.Order.InvalidTotalPrice);

        // Reset total price
        RemoveVoucher(appliedAtUtc);

        var orderVoucher = OrderVoucher.Create(
            orderId: Id,
            voucherId: voucherId,
            discountAmount: discountAmount,
            appliedAtUtc: appliedAtUtc);

        _orderVouchers.Add(orderVoucher);
        TotalPrice = Math.Max(0, OriginalTotalPrice - discountAmount);
        ModifiedAt = appliedAtUtc;

        return Result.Success();
    }

    public Result MarkPaid(DateTime? utcNow = null)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(TicketingErrors.Order.CannotMarkPaid(Status));

        if (_tickets.Count == 0)
            return Result.Failure(TicketingErrors.Order.NoTickets);

        Status = OrderStatus.Paid;
        ModifiedAt = utcNow ?? DateTime.UtcNow;

        var items = _tickets
            .Select(t => new OrderPaidTicketItem(
                t.Id,
                t.TicketTypeId,
                t.EventSessionId,
                t.SeatId,
                t.QrCode))
            .ToList();

        RaiseDomainEvent(new OrderPaidDomainEvent(Id, UserId, TotalPrice, items));
        return Result.Success();
    }

    public Result Cancel(DateTime? utcNow = null)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Paid))
            return Result.Failure(TicketingErrors.Order.CannotCancel(Status));

        if (_tickets.Any(t => t.Status == OrderTicketStatus.Used))
            return Result.Failure(TicketingErrors.Order.CannotCancelWithUsedTickets);

        foreach (var ticket in _tickets.Where(t => t.Status != OrderTicketStatus.Cancelled))
        {
            var cancelResult = ticket.Cancel(utcNow);
            if (cancelResult.IsFailure)
                return cancelResult;
        }

        Status = OrderStatus.Cancelled;
        ModifiedAt = utcNow ?? DateTime.UtcNow;

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, UserId));
        return Result.Success();
    }

    public Result CheckIn(Guid orderTicketId, Guid staffUserId, DateTime utcNow)
    {
        if (Status != OrderStatus.Paid)
            return Result.Failure(TicketingErrors.Order.NotPaid);

        var ticket = _tickets.FirstOrDefault(t => t.Id == orderTicketId);
        if (ticket is null)
            return Result.Failure(TicketingErrors.CheckIn.TicketNotFound);

        var checkInResult = ticket.CheckIn(staffUserId, utcNow);
        if (checkInResult.IsFailure)
            return checkInResult;
        return Result.Success();
    }

    public Result RemoveVoucher(DateTime? utcNow = null)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(TicketingErrors.Order.NotPending);

        var existing = _orderVouchers.FirstOrDefault();

        if (existing is null)
            return Result.Success();

        _orderVouchers.Remove(existing);

        TotalPrice = OriginalTotalPrice;
        ModifiedAt = utcNow ?? DateTime.UtcNow;

        return Result.Success();
    }

    protected override void Apply(IDomainEvent @event) { }
}
