using StackExchange.Redis;
using Events.Application.Abstractions;

namespace Events.Infrastructure.Services;

internal sealed class SeatLockService(IConnectionMultiplexer redis) : ISeatLockService
{
    private const string LockKeyPattern = "seat:lock:{0}:*";

    public async Task ReleaseAllLocksForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var pattern = string.Format(LockKeyPattern, eventId);

        var keys = new List<RedisKey>();

        foreach (var server in GetServers())
        {
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                cancellationToken.ThrowIfCancellationRequested();
                keys.Add(key);
            }
        }

        if (keys.Count == 0)
            return;

        await db.KeyDeleteAsync(keys.ToArray());
    }

    private IEnumerable<IServer> GetServers()
    {
        foreach (var endpoint in redis.GetEndPoints())
        {
            var server = redis.GetServer(endpoint);

            if (!server.IsConnected)
                continue;

            yield return server;
        }
    }
}
