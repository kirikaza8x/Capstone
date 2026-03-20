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

public sealed record EventSeatDto(
    Guid SeatId,
    Guid AreaId,
    string SeatCode);

public enum EventAreaType { Zone, Seat, Default }