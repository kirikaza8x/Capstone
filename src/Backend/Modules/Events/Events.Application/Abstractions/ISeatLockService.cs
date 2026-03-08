namespace Events.Application.Abstractions;

public interface ISeatLockService
{
    Task ReleaseAllLocksForEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}