namespace Events.PublicApi.Records;

public sealed record OrderTicketDetailDto(
    string TicketTypeName,
    decimal Price, 
    string SessionTitle,
    DateTime SessionStartTime,
    string? SeatCode);
