using Shared.Application.Abstractions.Messaging;

namespace Reports.Application.Admin.Queries.GetTopEvents;

public sealed record GetTopEventsQuery(int Top = 5) : IQuery<TopEventsResponse>;

public sealed record TopEventsResponse(List<TopEventDto> Events);

public sealed record TopEventDto(
    Guid EventId,
    string Title,
    string BannerUrl,
    string Status,
    decimal TotalRevenue,
    int TicketsSold);
