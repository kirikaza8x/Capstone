namespace Events.PublicApi.Records;

public sealed record TicketCheckInInfoDto(
    string TicketTypeName,
    string SessionTitle,
    DateTime SessionStartTime,
    string? SeatCode);
