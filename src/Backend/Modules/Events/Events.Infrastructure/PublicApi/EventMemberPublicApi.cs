using Events.Domain.Repositories;
using Events.PublicApi.PublicApi;

namespace Events.Infrastructure.PublicApi;

internal class EventMemberPublicApi(
    IEventRepository eventRepository
    ) : IEventMemberPublicApi
{
    public async Task<bool> HasPermissionAsync(Guid eventId, Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        return await eventRepository.HasPermissionAsync(eventId, userId, permission, cancellationToken);
    }
}
