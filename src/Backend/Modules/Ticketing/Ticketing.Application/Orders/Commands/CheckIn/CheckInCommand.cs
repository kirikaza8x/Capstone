using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Commands.CheckIn;

public sealed record CheckInCommand(
    string QrCode,
    Guid EventSessionId) : ICommand<CheckInResponse>;

public sealed record CheckInResponse(
    Guid OrderTicketId,
    string TicketTypeName,
    string SessionTitle,
    DateTime SessionStartTime,
    string? SeatCode,
    DateTime CheckedInAt);
