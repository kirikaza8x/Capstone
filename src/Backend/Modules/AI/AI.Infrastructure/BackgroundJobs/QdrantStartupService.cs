using AI.Application.Abstractions.Qdrant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AI.Infrastructure.Qdrant;

/// <summary>
/// Ensures Qdrant collections and payload indexes exist on startup.
/// Runs once at application start — all operations are idempotent.
///
/// Register in DI:
///   services.AddHostedService&lt;QdrantStartupService&gt;();
/// </summary>
public sealed class QdrantStartupService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QdrantStartupService> _logger;

    public QdrantStartupService(
        IServiceScopeFactory scopeFactory,
        ILogger<QdrantStartupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Initializing Qdrant collections...");

        // Repos are scoped — must create a scope to resolve them
        await using var scope = _scopeFactory.CreateAsyncScope();

        var eventRepo    = scope.ServiceProvider.GetRequiredService<IEventVectorRepository>();
        var behaviorRepo = scope.ServiceProvider.GetRequiredService<IUserBehaviorVectorRepository>();

        await eventRepo.EnsureCollectionAsync(ct);
        await behaviorRepo.EnsureCollectionAsync(ct);

        _logger.LogInformation("✅ Qdrant collections ready");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}