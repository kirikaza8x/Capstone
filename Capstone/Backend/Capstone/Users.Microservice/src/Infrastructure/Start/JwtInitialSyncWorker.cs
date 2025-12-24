using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // Required for CreateScope
using Shared.Application.Abstractions.Authentication;
using Shared.Contracts.Events.Configs;

namespace Users.Infrastructure.Startup;

public class JwtInitialSyncWorker : BackgroundService
{
    private readonly IDistributedCache _cache;
    private readonly IConfigurableJwtService _jwtService;
    private readonly ILogger<JwtInitialSyncWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory; // Fixed: Use factory to handle Scoped services

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
        // Match this to your Docker environment (Development)
        const string cacheKey = "config:jwt:Development"; 

        _logger.LogInformation("Users Service: Starting JWT Configuration initial sync...");

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

                var cachedData = await _cache.GetStringAsync(cacheKey, timeoutCts.Token);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var settings = JsonSerializer.Deserialize<JwtConfigurationChangedEvent>(cachedData);
                    if (settings != null)
                    {
                        await _jwtService.UpdateConfigurationAsync(settings.ExpiryMinutes, settings.RefreshTokenExpiryDays);
                        _logger.LogInformation("Users Service: Successfully initialized via Redis.");
                        return; 
                    }
                }

                _logger.LogInformation("Redis empty/unavailable. Requesting config from ConfigsDB via Message Bus...");
                
                using (var scope = _scopeFactory.CreateScope())
                {
                    var client = scope.ServiceProvider.GetRequiredService<IRequestClient<IGetJwtConfigurationRequest>>();
                    var response = await client.GetResponse<JwtConfigurationChangedEvent>(new { }, timeoutCts.Token);
                    
                    await _jwtService.UpdateConfigurationAsync(
                        response.Message.ExpiryMinutes, 
                        response.Message.RefreshTokenExpiryDays);
                }
                
                _logger.LogInformation("Users Service: Successfully initialized via ConfigsDB.");
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning("Startup sync attempt {Count} failed: {Message}", retryCount, ex.Message);
                
                if (retryCount < maxRetries) 
                {
                    await Task.Delay(3000, stoppingToken);
                }
            }
        }

        _logger.LogWarning("All startup sync attempts failed. Reverting to Emergency Defaults.");
        _jwtService.UseEmergencyDefaults();
    }
}