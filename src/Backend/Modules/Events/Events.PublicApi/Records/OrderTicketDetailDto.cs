namespace Events.PublicApi.Records;

public sealed record OrderTicketDetailDto(
    string TicketTypeName,
    string SessionTitle,
    DateTime SessionStartTime,
    string? SeatCode);
