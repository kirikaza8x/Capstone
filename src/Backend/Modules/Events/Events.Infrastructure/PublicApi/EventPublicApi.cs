using Events.Domain.Enums;
using Events.Infrastructure.Data;
using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Abstractions.Time;

namespace Events.Infrastructure.PublicApi;

internal sealed class EventPublicApi(
    EventsDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IEventPublicApi
{
    public async Task<EventMetricsDto> GetEventMetricsAsync(CancellationToken cancellationToken = default)
    {
        var totalEvents = await dbContext.Events.CountAsync(cancellationToken);

        var now = dateTimeProvider.UtcNow;

        var liveEventsNow = await dbContext.Events
            .CountAsync(e =>
                e.Status == EventStatus.Published &&
                e.EventStartAt <= now &&
                e.EventEndAt >= now,
                cancellationToken);

        return new EventMetricsDto(
            TotalEvents: totalEvents,
            LiveEventsNow: liveEventsNow);
    }

    public async Task<Dictionary<Guid, EventBasicInfoDto>> GetEventMapByIdsAsync(
        IEnumerable<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        var idList = eventIds.ToList();

        if (idList.Count == 0)
            return new Dictionary<Guid, EventBasicInfoDto>();

        var events = await dbContext.Events
            .Where(e => idList.Contains(e.Id))
            .Select(e => new EventBasicInfoDto(
                e.Id,
                e.Title,
                e.BannerUrl ?? "",
                e.Status.ToString()))
            .ToListAsync(cancellationToken);

        return events.ToDictionary(e => e.Id);
    }
}
