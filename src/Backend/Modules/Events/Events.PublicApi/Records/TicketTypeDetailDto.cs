namespace Events.PublicApi.Records;

public sealed record TicketTypeDetailDto(
    Guid Id,
    string Name,
    decimal Price,
    int Quantity);
