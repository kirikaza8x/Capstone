using Events.Domain.DomainEvents;
using Events.Domain.Enums;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class Seat : Entity<Guid>
{
    private Seat() { }

    public Guid AreaId { get; private set; }
    public string SeatCode { get; private set; } = string.Empty;
    public string RowLabel { get; private set; } = string.Empty;
    public string ColumnLabel { get; private set; } = string.Empty;
    public float X { get; private set; }
    public float Y { get; private set; }
    public SeatStatus Status { get; private set; }

    public Area Area { get; private set; } = null!;

    public static Seat Create(
        Guid areaId,
        string seatCode,
        string rowLabel,
        string columnLabel,
        float x,
        float y)
    {
        return new Seat
        {
            Id = Guid.NewGuid(),
            AreaId = areaId,
            SeatCode = seatCode,
            RowLabel = rowLabel,
            ColumnLabel = columnLabel,
            X = x,
            Y = y,
            Status = SeatStatus.Available,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Reserve()
    {
        if (Status != SeatStatus.Available)
            throw new InvalidOperationException("Seat is not available for reservation.");

        Status = SeatStatus.Reserved;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Sell()
    {
        if (Status != SeatStatus.Reserved)
            throw new InvalidOperationException("Seat must be reserved before selling.");

        Status = SeatStatus.Sold;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        Status = SeatStatus.Available;
        ModifiedAt = DateTime.UtcNow;
    }
}