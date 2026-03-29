using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Queries.GetCheckInStats;

public sealed record GetCheckInStatsQuery(
    Guid EventId,
    Guid EventSessionId) : IQuery<CheckInStatsResponse>;

public sealed record CheckInStatsResponse(
    CheckInSummary Summary,
    IReadOnlyCollection<TicketTypeStat> TicketStats);

public sealed record CheckInSummary(
    int TotalTickets,
    int TotalQuantity,
    int CheckedIn);

public sealed record TicketTypeStat(
    string TicketType,
    int Quantity,
    int CheckedIn);
