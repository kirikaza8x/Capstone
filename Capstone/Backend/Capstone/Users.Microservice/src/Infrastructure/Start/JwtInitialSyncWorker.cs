using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Abstractions.Authentication;
using Shared.Contracts.Events.Configs;

namespace Users.Infrastructure.Startup;

public class JwtInitialSyncWorker : BackgroundService
{
    private readonly IDistributedCache _cache;
    private readonly IConfigurableJwtService _jwtService;
    private readonly ILogger<JwtInitialSyncWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public JwtInitialSyncWorker(
        IDistributedCache cache, 
        IConfigurableJwtService jwtService, 
        ILogger<JwtInitialSyncWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _cache = cache;
        _jwtService = jwtService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int maxRetries = 3;
        int retryCount = 0;
        const string cacheKey = "config:jwt:Development"; 

        _logger.LogInformation("[SYNC] Starting JWT Configuration initial sync...");

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Increased timeout to 10s for local debugging stability
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

                _logger.LogDebug("[SYNC] Checking Redis for key: {Key}", cacheKey);
                var cachedData = await _cache.GetStringAsync(cacheKey, timeoutCts.Token);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("[SYNC] Found config in Redis. Deserializing...");
                    var settings = JsonSerializer.Deserialize<JwtConfigurationChangedEvent>(cachedData);
                    
                    if (settings != null)
                    {
                        await _jwtService.UpdateConfigurationAsync(settings.ExpiryMinutes, settings.RefreshTokenExpiryDays);
                        _logger.LogInformation("[SYNC] SUCCESS: Initialized via Redis. (Expiry: {Exp}m)", settings.ExpiryMinutes);
                        return; 
                    }
                }

                _logger.LogInformation("[SYNC] Redis empty/unavailable. Requesting from ConfigsDB via RabbitMQ...");
                
                using (var scope = _scopeFactory.CreateScope())
                {
                    var client = scope.ServiceProvider.GetRequiredService<IRequestClient<IGetJwtConfigurationRequest>>();
                    
                    _logger.LogDebug("[SYNC] Sending IGetJwtConfigurationRequest to bus...");
                    
                    // Sending the request and waiting for response
                    var response = await client.GetResponse<IJwtConfigurationResponse>(new { }, timeoutCts.Token);
                    
                    _logger.LogInformation("[SYNC] Received Response from Bus! New Expiry: {Exp}m, Refresh: {Ref}d", 
                        response.Message.ExpiryMinutes, 
                        response.Message.RefreshTokenExpiryDays);

                    await _jwtService.UpdateConfigurationAsync(
                        response.Message.ExpiryMinutes, 
                        response.Message.RefreshTokenExpiryDays);
                }
                
                _logger.LogInformation("[SYNC] SUCCESS: Initialized via ConfigsDB.");
                return;
            }
            catch (OperationCanceledException)
            {
                retryCount++;
                _logger.LogWarning("[SYNC] Attempt {Count} TIMEOUT: ConfigsDB did not respond within 10s.", retryCount);
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogCritical(ex, "[SYNC] Attempt {Count} ERROR: {Message}", retryCount, ex.Message);
            }

            if (retryCount < maxRetries && !stoppingToken.IsCancellationRequested) 
            {
                _logger.LogInformation("[SYNC] Retrying in 3 seconds...");
                await Task.Delay(3000, stoppingToken);
            }
        }

        _logger.LogError("[SYNC] FATAL: All sync attempts failed. Reverting to Emergency Defaults.");
        _jwtService.UseEmergencyDefaults();
    }
}