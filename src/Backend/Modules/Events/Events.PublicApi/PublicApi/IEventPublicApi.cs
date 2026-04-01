using Events.PublicApi.Records;

namespace Events.PublicApi.PublicApi;

public interface IEventPublicApi
{
    Task<EventMetricsDto> GetEventMetricsAsync(CancellationToken cancellationToken = default);
}
