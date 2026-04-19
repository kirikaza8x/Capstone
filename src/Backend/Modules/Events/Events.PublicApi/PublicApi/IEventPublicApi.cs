using Events.PublicApi.Records;

namespace Events.PublicApi.PublicApi;

public interface IEventPublicApi
{
    Task<IReadOnlyList<Guid>> GetPublishedEventIdsAsync(CancellationToken cancellationToken = default);
    Task<EventMetricsDto> GetEventMetricsAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, EventBasicInfoDto>> GetEventMapByIdsAsync(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default);
}
