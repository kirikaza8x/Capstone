using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Commands.ManualCheckIn;

public sealed record ManualCheckInCommand(
    Guid EventId,
    Guid EventSessionId,
    List<Guid> OrderTicketIds) : ICommand<ManualCheckInResponse>;

public sealed record ManualCheckInResponse(int TotalSuccess);
