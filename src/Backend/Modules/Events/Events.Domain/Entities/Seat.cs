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
            CreatedAt = DateTime.UtcNow
        };
    }
}