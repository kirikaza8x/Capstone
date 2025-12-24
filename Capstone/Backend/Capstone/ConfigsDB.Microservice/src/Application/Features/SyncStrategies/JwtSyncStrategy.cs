namespace ConfigsDB.Application.Features.ConfigSettings.SyncStrategies;

using System.Text.Json;
using ConfigsDB.Application.Abstractions.Configs;
using ConfigsDB.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Application.Events;
using Shared.Contracts.Events.Configs;

public class JwtSyncStrategy : IConfigSyncStrategy
{
    private readonly IConfigSettingRepository _repository;
    private readonly IServiceBusPublisher _publisher;
    private readonly IDistributedCache _cache;

    public JwtSyncStrategy(IConfigSettingRepository repository, IServiceBusPublisher publisher, IDistributedCache cache)
    {
        _repository = repository;
        _publisher = publisher;
        _cache = cache;
    }

    public bool CanHandle(string key) => key.StartsWith("Jwt.", StringComparison.OrdinalIgnoreCase);

    public async Task SyncAsync(string environment, CancellationToken ct)
    {
        // 1. Fetch Full State from Repo
        var expiry = await _repository.GetByKeyAndEnvironmentAsync("Jwt.ExpiryMinutes", environment, ct);
        var refresh = await _repository.GetByKeyAndEnvironmentAsync("Jwt.RefreshDays", environment, ct);

        var syncEvent = new JwtConfigurationChangedEvent {
            ExpiryMinutes = int.TryParse(expiry?.Value, out var e) ? e : 60,
            RefreshTokenExpiryDays = int.TryParse(refresh?.Value, out var r) ? r : 7,
            Environment = environment
        };

        // 2. Update Redis (The Drop Box)
        var cacheKey = $"config:jwt:{environment}";
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(syncEvent), ct);

        // 3. Publish to RabbitMQ (The Megaphone)
        await _publisher.PublishAsync(syncEvent, ct);
    }
}