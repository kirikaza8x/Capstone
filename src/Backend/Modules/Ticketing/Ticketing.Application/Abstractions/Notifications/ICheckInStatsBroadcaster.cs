namespace Ticketing.Application.Abstractions.Notifications;

public interface ICheckInStatsBroadcaster
{
    Task BroadcastAsync(
        Guid eventId,
        Guid eventSessionId,
        CancellationToken cancellationToken = default);
}
