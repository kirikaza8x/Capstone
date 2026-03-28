using Events.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace Events.Infrastructure.Caching;

internal sealed class EventMemberPermissionCacheInvalidator(
    IDistributedCache cache) : IEventMemberPermissionCacheInvalidator
{
    public async Task InvalidateAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = EventPermissionCacheKeys.UserPermissions(eventId, userId);
        await cache.RemoveAsync(cacheKey, cancellationToken);
    }
}
