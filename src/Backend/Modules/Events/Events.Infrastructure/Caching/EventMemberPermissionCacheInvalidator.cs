using Events.Application.Abstractions.Caching;
using Events.PublicApi.Constants;
using Microsoft.Extensions.Caching.Distributed;

namespace Events.Infrastructure.Caching;

internal sealed class EventMemberPermissionCacheInvalidator(
    IDistributedCache distributedCache) : IEventMemberPermissionCacheInvalidator
{
    public async Task InvalidateAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        foreach (var permission in EventMemberPermission.All)
        {
            var cacheKey = EventPermissionCacheKeys.Permission(eventId, userId, permission);
            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
        }
    }
}
