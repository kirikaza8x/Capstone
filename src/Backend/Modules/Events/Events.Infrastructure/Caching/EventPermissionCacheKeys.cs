namespace Events.Infrastructure.Caching;

internal static class EventPermissionCacheKeys
{
    public static string UserPermissions(Guid eventId, Guid userId)
            => $"event_permissions:{eventId}:{userId}";
}
