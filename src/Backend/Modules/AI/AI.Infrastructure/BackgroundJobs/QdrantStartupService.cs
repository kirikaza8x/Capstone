using AI.Application.Abstractions.Qdrant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Configs.Qdrant;

namespace AI.Infrastructure.Qdrant;

public sealed class QdrantStartupService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QdrantStartupService> _logger;
    private readonly QdrantConfig _config;
    private Task? _backgroundTask;
    private CancellationTokenSource? _cts;

    public QdrantStartupService(
        IServiceScopeFactory scopeFactory,
        ILogger<QdrantStartupService> logger,
        IOptions<QdrantConfig> config)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
        _config       = config.Value;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting Qdrant initialization loop...");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _backgroundTask = Task.Run(() => RetryLoopAsync(_cts.Token));
        return Task.CompletedTask; // don’t block startup
    }

    private async Task RetryLoopAsync(CancellationToken ct)
    {
        int delay = _config.Retry.InitialDelaySeconds;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var eventRepo    = scope.ServiceProvider.GetRequiredService<IEventVectorRepository>();
                var behaviorRepo = scope.ServiceProvider.GetRequiredService<IUserBehaviorVectorRepository>();

                await eventRepo.EnsureCollectionAsync(ct);
                await behaviorRepo.EnsureCollectionAsync(ct);

                _logger.LogInformation("✅ Qdrant collections ready");
                return; // exit loop once successful
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Qdrant not ready yet, retrying in {Delay}s...", delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), ct);

                // increase delay up to max
                delay = Math.Min(delay * 2, _config.Retry.MaxDelaySeconds);

                // if Infinite=false, break after some retries
                if (!_config.Retry.Infinite && delay >= _config.Retry.MaxDelaySeconds)
                    break;
            }
        }
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_cts != null)
        {
            _cts.Cancel();
            if (_backgroundTask != null)
                await _backgroundTask;
        }
    }
}
