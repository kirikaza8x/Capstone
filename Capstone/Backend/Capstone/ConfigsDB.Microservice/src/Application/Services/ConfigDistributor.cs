namespace ConfigsDB.Application.Services;

using ConfigsDB.Application.Abstractions.Configs;

public class ConfigDistributor : IConfigDistributor
{
    private readonly IEnumerable<IConfigSyncStrategy> _strategies;

    public ConfigDistributor(IEnumerable<IConfigSyncStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async Task DistributeAsync(string key, string environment, CancellationToken ct)
    {
        // Find the strategy that "CanHandle" this key
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(key));
        
        if (strategy != null)
        {
            await strategy.SyncAsync(environment, ct);
        }
    }
}