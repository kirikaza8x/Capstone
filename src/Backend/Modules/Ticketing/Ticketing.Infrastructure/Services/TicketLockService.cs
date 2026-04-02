using StackExchange.Redis;
using Ticketing.Application.Abstractions.Locks;

namespace Ticketing.Infrastructure.Services;

internal sealed class TicketLockService(IConnectionMultiplexer redis) : ITicketLockService
{
    private static readonly string ZoneIncrScript = @"
        local current = tonumber(redis.call('GET', KEYS[1]) or '0')
        local maxAllowed = tonumber(ARGV[1])
        local increaseBy = tonumber(ARGV[2])
        local ttl = tonumber(ARGV[3])

        if current + increaseBy > maxAllowed then
            return 0
        end

        local newVal = redis.call('INCRBY', KEYS[1], increaseBy)
        redis.call('EXPIRE', KEYS[1], ttl)
        return 1
    ";

    private static readonly string ZoneDecrScript = @"
        local current = tonumber(redis.call('GET', KEYS[1]) or '0')
        local decreaseBy = tonumber(ARGV[1])

        local newVal = current - decreaseBy
        if newVal <= 0 then
            redis.call('DEL', KEYS[1])
            return 0
        end

        local ttl = redis.call('TTL', KEYS[1])
        redis.call('SET', KEYS[1], newVal)
        if ttl > 0 then
            redis.call('EXPIRE', KEYS[1], ttl)
        end
        return newVal
    ";

    private readonly IDatabase _db = redis.GetDatabase();

    // Key conventions
    // seat_lock:{eventSessionId}:{seatId}
    private static string SeatKey(Guid sessionId, Guid seatId) =>
        $"seat_lock:{sessionId}:{seatId}";

    // zone_lock:{eventSessionId}:{ticketTypeId}
    private static string ZoneKey(Guid sessionId, Guid ticketTypeId) =>
        $"zone_lock:{sessionId}:{ticketTypeId}";

    public async Task<bool> TryLockSeatAsync(
        Guid eventSessionId,
        Guid seatId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        return await _db.StringSetAsync(
            SeatKey(eventSessionId, seatId),
            userId.ToString(),
            ttl,
            When.NotExists);
    }

    //release lock
    public async Task UnlockSeatAsync(
        Guid eventSessionId,
        Guid seatId,
        CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(SeatKey(eventSessionId, seatId));
    }

    public async Task<bool> TryIncreaseZoneLockAsync(
        Guid eventSessionId,
        Guid ticketTypeId,
        int increaseBy,
        int maxAllowed,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var result = (int)await _db.ScriptEvaluateAsync(
            ZoneIncrScript,
            keys: [ZoneKey(eventSessionId, ticketTypeId)],
            values: [maxAllowed, increaseBy, (int)ttl.TotalSeconds]);

        return result == 1;
    }

    public async Task DecreaseZoneLockAsync(
        Guid eventSessionId,
        Guid ticketTypeId,
        int decreaseBy,
        CancellationToken cancellationToken = default)
    {
        await _db.ScriptEvaluateAsync(
            ZoneDecrScript,
            keys: [ZoneKey(eventSessionId, ticketTypeId)],
            values: [decreaseBy]);
    }
}
