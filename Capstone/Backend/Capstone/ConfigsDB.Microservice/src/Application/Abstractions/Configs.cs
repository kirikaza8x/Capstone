namespace ConfigsDB.Application.Abstractions.Configs;

/// <summary>
/// Defines a strategy for synchronizing specific configuration types 
/// across the service bus.
/// </summary>
public interface IConfigSyncStrategy
{
    /// <summary>
    /// Checks if this strategy handles the specific key (e.g., starts with "Jwt.")
    /// </summary>
    bool CanHandle(string key);
    
    /// <summary>
    /// Executes the logic to fetch full state and publish the integration event.
    /// </summary>
    Task SyncAsync(string environment, CancellationToken ct);
}

/// <summary>
/// The orchestrator that picks the right strategy based on the changed key.
/// </summary>
public interface IConfigDistributor
{
    Task DistributeAsync(string key, string environment, CancellationToken ct);
}