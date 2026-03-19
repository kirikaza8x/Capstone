namespace Ticketing.Application.Abstractions.Locks;

public interface ITicketLockService
{
    Task<bool> TryLockSeatAsync(
        Guid eventSessionId,
        Guid seatId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task UnlockSeatAsync(
        Guid eventSessionId,
        Guid seatId,
        CancellationToken cancellationToken = default);

    Task<int> GetZoneLockedCountAsync(
        Guid eventSessionId,
        Guid ticketTypeId,
        CancellationToken cancellationToken = default);

    Task<bool> TryIncreaseZoneLockAsync(
        Guid eventSessionId,
        Guid ticketTypeId,
        int increaseBy,
        int maxAllowed,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task DecreaseZoneLockAsync(
        Guid eventSessionId,
        Guid ticketTypeId,
        int decreaseBy,
        CancellationToken cancellationToken = default);
}