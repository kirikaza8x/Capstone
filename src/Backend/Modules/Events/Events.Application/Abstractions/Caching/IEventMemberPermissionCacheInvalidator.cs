namespace Events.Application.Abstractions.Caching;

public interface IEventMemberPermissionCacheInvalidator
{
    Task InvalidateAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
}
