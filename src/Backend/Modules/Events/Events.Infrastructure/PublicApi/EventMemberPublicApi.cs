using Events.Domain.Repositories;
using Events.Infrastructure.Caching;
using Events.PublicApi.Constants;
using Events.PublicApi.PublicApi;
using Microsoft.Extensions.Caching.Distributed;

namespace Events.Infrastructure.PublicApi;

internal class EventMemberPublicApi(
    IEventRepository eventRepository,
    IDistributedCache distributedCache
    ) : IEventMemberPublicApi
{
    private static readonly TimeSpan PermissionCacheTtl = TimeSpan.FromSeconds(60);

    public async Task<bool> HasPermissionAsync(Guid eventId, Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        if (!EventPermissions.All.Contains(permission))
            return false;

        var cacheKey = EventPermissionCacheKeys.Permission(eventId, userId, permission);

        var cachedValue = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (cachedValue is not null)
            return cachedValue == bool.TrueString;

        var hasPermission = await eventRepository.HasPermissionAsync(eventId, userId, permission, cancellationToken);

        await distributedCache.SetStringAsync(
            cacheKey,
            hasPermission.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PermissionCacheTtl
            },
            cancellationToken);

        return hasPermission;
    }
}
