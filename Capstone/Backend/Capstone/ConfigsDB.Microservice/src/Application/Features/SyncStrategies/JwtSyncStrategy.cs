namespace ConfigsDB.Application.Features.ConfigSettings.SyncStrategies;

using ConfigsDB.Application.Abstractions.Configs;
using ConfigsDB.Domain.Repositories;
using Shared.Application.Events;
using Shared.Contracts.Events.Configs;

public class JwtSyncStrategy : IConfigSyncStrategy
{
    private readonly IConfigSettingRepository _repository;
    private readonly IServiceBusPublisher _publisher;

    public JwtSyncStrategy(IConfigSettingRepository repository, IServiceBusPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public bool CanHandle(string key) => 
        key.StartsWith("Jwt.", StringComparison.OrdinalIgnoreCase);

    public async Task SyncAsync(string environment, CancellationToken ct)
    {
        // Fetch all related keys to send a complete state
        var expiry = await _repository.GetByKeyAndEnvironmentAsync("Jwt.ExpiryMinutes", environment, ct);
        var refresh = await _repository.GetByKeyAndEnvironmentAsync("Jwt.RefreshDays", environment, ct);

        var syncEvent = new JwtConfigurationChangedEvent
        {
            CorrelationId = Guid.NewGuid(),
            SourceService = "ConfigsDB",
            OccurredAt = DateTime.UtcNow,
            ExpiryMinutes = int.TryParse(expiry?.Value, out var e) ? e : 60,
            RefreshTokenExpiryDays = int.TryParse(refresh?.Value, out var r) ? r : 7
        };

        await _publisher.PublishAsync(syncEvent, ct);
    }
}