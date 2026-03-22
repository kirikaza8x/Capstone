using Shared.Domain.Abstractions;
using Shared.Domain.DDD;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;

namespace Ticketing.Domain.Entities;

public sealed class OrderTicket : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid EventSessionId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public Guid? SeatId { get; private set; }
    public string QrCode { get; private set; } = string.Empty;
    public OrderTicketStatus Status { get; private set; }
    public DateTime? CheckedInAt { get; private set; }
    public Guid? CheckedInBy { get; private set; }

    public Order Order { get; private set; } = null!;

    private OrderTicket() { }

    public static Result<OrderTicket> Create(
        Guid orderId,
        Guid eventSessionId,
        Guid ticketTypeId,
        Guid? seatId,
        string qrCode,
        Guid? id = null,
        DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
            return Result.Failure<OrderTicket>(TicketingErrors.OrderTicket.InvalidQrCode);

        var now = utcNow ?? DateTime.UtcNow;
        var entity = new OrderTicket
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId,
            EventSessionId = eventSessionId,
            TicketTypeId = ticketTypeId,
            SeatId = seatId,
            QrCode = qrCode.Trim(),
            Status = OrderTicketStatus.Valid,
            CreatedAt = now
        };
        return Result.Success(entity);
    }

    public Result Cancel(DateTime? utcNow = null)
    {
        if (Status == OrderTicketStatus.Used)
            return Result.Failure(TicketingErrors.OrderTicket.CannotCancel(Status));

        if (Status == OrderTicketStatus.Cancelled)
            return Result.Success();

        Status = OrderTicketStatus.Cancelled;
        ModifiedAt = utcNow ?? DateTime.UtcNow;
        return Result.Success();
    }

    public Result CheckIn(Guid staffUserId, DateTime utcNow)
    {
        if (Status == OrderTicketStatus.Used)
            return Result.Failure(TicketingErrors.CheckIn.AlreadyCheckedIn);

        if (Status == OrderTicketStatus.Cancelled)
            return Result.Failure(TicketingErrors.CheckIn.TicketCancelled);

        if (Status != OrderTicketStatus.Valid)
            return Result.Failure(TicketingErrors.CheckIn.InvalidTicketStatus);

        Status = OrderTicketStatus.Used;
        CheckedInAt = utcNow;
        CheckedInBy = staffUserId;

        return Result.Success();
    }
}
