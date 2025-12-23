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
        var expiry = await _repository.GetByKeyAndEnvironmentAsync("Jwt.ExpiryMinutes", environment, ct);
        var refresh = await _repository.GetByKeyAndEnvironmentAsync("Jwt.RefreshDays", environment, ct);

        var syncEvent = new JwtConfigurationChangedEvent {
            ExpiryMinutes = int.TryParse(expiry?.Value, out var e) ? e : 60,
            RefreshTokenExpiryDays = int.TryParse(refresh?.Value, out var r) ? r : 7,
            Environment = environment
        };

        var cacheKey = $"config:jwt:{environment}";
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(syncEvent), ct);

        await _publisher.PublishAsync(syncEvent, ct);
    }
}