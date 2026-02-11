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
            Status = SeatStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate() => Status = SeatStatus.Active;
    public void Deactivate() => Status = SeatStatus.Inactive;
    public void SetMaintenance() => Status = SeatStatus.Maintenance;
}