namespace Events.PublicApi.Records;

public sealed record EventTicketingItemDto(
    Guid EventId,
    Guid EventSessionId,
    Guid TicketTypeId,
    Guid? AreaId,
    EventAreaType AreaType,
    decimal Price,
    int Quantity,
    int SoldQuantity,
    bool IsPurchasable);

<<<<<<< HEAD
public sealed record EventSeatDto(
    Guid SeatId,
    Guid AreaId,
    string SeatCode);

public enum EventAreaType { Zone, Seat, Default }
=======
public sealed record EventTicketingItemDto
{
    public Guid EventId { get; init; }
    public Guid EventSessionId { get; init; }
    public Guid TicketTypeId { get; init; }
    public Guid? AreaId { get; init; }
    public EventAreaType AreaType { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public int SoldQuantity { get; init; }
    public bool IsPurchasable { get; init; }
}
>>>>>>> 97ebaa9 (chore: add editorconfig and fix final newlines)
