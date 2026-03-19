namespace Events.PublicApi.Records;

public enum EventAreaType
{
    Zone = 0,
    Seat = 1,
    Default = 2
}

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