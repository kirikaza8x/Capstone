namespace Events.PublicApi.Records;

public sealed record EventSeatDto
{
    public Guid SeatId { get; init; }
    public Guid AreaId { get; init; }
}
