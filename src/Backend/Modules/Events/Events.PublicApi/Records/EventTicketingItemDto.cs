namespace Events.PublicApi.Records;

public sealed record EventTicketingItemDto(
    Guid EventId,
    Guid EventSessionId,
    Guid TicketTypeId,
    Guid? AreaId,
    EventAreaType AreaType,
    decimal Price,
    int Quantity,
    bool IsPurchasable,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Hashtags);

public sealed record EventSeatDto(
    Guid SeatId,
    Guid AreaId,
    string SeatCode);

public enum EventAreaType { Zone, Seat, Default }
