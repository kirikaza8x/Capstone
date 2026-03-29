using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Queries.GetTicketsForCheckIn;

public sealed record GetTicketsForCheckInQuery(
    Guid EventId,
    Guid EventSessionId,
    string Email) : IQuery<IReadOnlyCollection<TicketForCheckInResponse>>;

public sealed record TicketForCheckInResponse(
    Guid OrderTicketId,
    string TicketTypeName,
    string? SeatCode,
    bool IsCheckedIn,
    DateTime? CheckedInAt);
