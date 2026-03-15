namespace Events.Infrastructure.Caching;

internal static class EventPermissionCacheKeys
{
    public static string Permission(Guid eventId, Guid userId, string permission)
    {
        return $"event_permission:{eventId}:{userId}:{permission}";
    }
}