namespace Events.PublicApi.PublicApi;

public interface IEventMemberPublicApi
{
    Task<bool> HasPermissionAsync(
        Guid eventId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);
}
