using Events.Application.Abstractions;
using StackExchange.Redis;

namespace Events.Infrastructure.Services;

internal sealed class SeatLockService(IConnectionMultiplexer redis) : ISeatLockService
{
    // Key format: seat:lock:{eventId}:{sessionId}:{seatId}
    private const string EventLockPattern = "seat:lock:{0}:*";
    private const int ScanPageSize = 1000;
    private const int DeleteBatchSize = 500;

    public async Task ReleaseAllLocksForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var pattern = string.Format(EventLockPattern, eventId);
        await DeleteByPatternAsync(pattern, cancellationToken);
    }

    private async Task DeleteByPatternAsync(string pattern, CancellationToken cancellationToken)
    {
        var db = redis.GetDatabase();

        foreach (var server in GetWritableServers())
        {
            var batch = new List<RedisKey>(DeleteBatchSize);

            await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: ScanPageSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                batch.Add(key);

                if (batch.Count >= DeleteBatchSize)
                {
                    await db.KeyDeleteAsync([.. batch]);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
                await db.KeyDeleteAsync([.. batch]);
        }
    }

    private IEnumerable<IServer> GetWritableServers()
    {
        foreach (var endpoint in redis.GetEndPoints())
        {
            var server = redis.GetServer(endpoint);

            if (!server.IsConnected || server.IsReplica)
                continue;

            yield return server;
        }
    }
}