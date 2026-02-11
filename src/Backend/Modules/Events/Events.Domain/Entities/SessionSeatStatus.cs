using Events.Domain.Enums;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class SessionSeatStatus : Entity<Guid>
{
    private SessionSeatStatus() { }

    public Guid EventSessionId { get; private set; }
    public Guid SeatId { get; private set; }
    public SessionSeatStatusType Status { get; private set; }
    public Guid? UserId { get; private set; }

    public EventSession EventSession { get; private set; } = null!;
    public Seat Seat { get; private set; } = null!;

    public static SessionSeatStatus Create(
        Guid eventSessionId,
        Guid seatId)
    {
        return new SessionSeatStatus
        {
            Id = Guid.NewGuid(),
            EventSessionId = eventSessionId,
            SeatId = seatId,
            Status = SessionSeatStatusType.Available,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Lock(Guid userId)
    {
        if (Status != SessionSeatStatusType.Available)
            throw new InvalidOperationException("Seat is not available.");
        Status = SessionSeatStatusType.Locked;
        UserId = userId;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Sell(Guid userId)
    {
        if (Status != SessionSeatStatusType.Locked)
            throw new InvalidOperationException("Seat must be locked before selling.");
        Status = SessionSeatStatusType.Sold;
        UserId = userId;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        Status = SessionSeatStatusType.Available;
        UserId = null;
        ModifiedAt = DateTime.UtcNow;
    }
}