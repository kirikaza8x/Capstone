using Events.PublicApi.PublicApi;
using Reports.Application.Admin.Queries.GetTopEvents;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;

namespace Reports.Application.AdminDashboards.Queries.GetTopEvents;

internal sealed class GetTopEventsQueryHandler(
    ITicketingPublicApi ticketingApi,
    IEventPublicApi eventApi,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetTopEventsQuery, TopEventsResponse>
{
    public async Task<Result<TopEventsResponse>> Handle(
        GetTopEventsQuery query,
        CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = dateTimeProvider.UtcNow.AddDays(-30);

        var ticketingMetrics = await ticketingApi.GetTopEventsMetricsAsync(
            query.Top,
            thirtyDaysAgo,
            cancellationToken);

        if (ticketingMetrics.Count == 0)
        {
            return Result.Success(new TopEventsResponse(new List<TopEventDto>()));
        }

        var eventIds = ticketingMetrics.Select(m => m.EventId).ToList();

        // get event details
        var eventDetailsMap = await eventApi.GetEventMapByIdsAsync(eventIds, cancellationToken);

        var resultList = ticketingMetrics.Select(metrics =>
        {
            eventDetailsMap.TryGetValue(metrics.EventId, out var details);

            return new TopEventDto(
                EventId: metrics.EventId,
                Title: details?.Title ?? "N/A",
                BannerUrl: details?.BannerUrl ?? "",
                Status: details?.Status ?? "Unknown",
                TotalRevenue: metrics.TotalRevenue,
                TicketsSold: metrics.TicketsSold
            );
        }).ToList();

        return Result.Success(new TopEventsResponse(resultList));
    }
}
