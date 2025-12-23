// using System.Text.Json;
// using MassTransit;
// using Microsoft.Extensions.Caching.Distributed;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Shared.Application.Abstractions.Authentication;
// using Shared.Contracts.Events.Configs;

// public class JwtInitialSyncWorker : BackgroundService
// {
//     private readonly IDistributedCache _cache;
//     private readonly IRequestClient<IGetJwtConfigurationRequest> _client;
//     private readonly IJwtTokenService _jwtService;
//     private readonly ILogger<JwtInitialSyncWorker> _logger;

//     public JwtInitialSyncWorker(IDistributedCache cache, IRequestClient<IGetJwtConfigurationRequest> client, IJwtTokenService jwtService, ILogger<JwtInitialSyncWorker> logger)
//     {
//         _cache = cache; _client = client; _jwtService = jwtService; _logger = logger;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         const int maxRetries = 3;
//         int retryCount = 0;
//         string cacheKey = "config:jwt:Production"; 

//         while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
//                 timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

//                 var cachedData = await _cache.GetStringAsync(cacheKey, timeoutCts.Token);
//                 if (!string.IsNullOrEmpty(cachedData))
//                 {
//                     var settings = JsonSerializer.Deserialize<JwtConfigurationChangedEvent>(cachedData);
//                     await _jwtService.UpdateConfigurationAsync(settings.ExpiryMinutes, settings.RefreshTokenExpiryDays);
//                     _logger.LogInformation("Users Service: Initialized via Redis.");
//                     return; 
//                 }

//                 // 🟡 2. FALLBACK: REQUEST/RESPONSE
//                 var response = await _client.GetResponse<IJwtConfigurationResponse>(new { }, timeoutCts.Token);
//                 await _jwtService.UpdateConfigurationAsync(response.Message.ExpiryMinutes, response.Message.RefreshTokenExpiryDays);
//                 _logger.LogInformation("Users Service: Initialized via ConfigsDB.");
//                 return;
//             }
//             catch (Exception ex)
//             {
//                 retryCount++;
//                 _logger.LogWarning("Startup attempt {Count} failed. Retrying...", retryCount);
//                 if (retryCount < maxRetries) await Task.Delay(3000, stoppingToken);
//             }
//         }
//         _logger.LogCritical("All startup attempts failed. Using Emergency Defaults.");
//         _jwtService.UseEmergencyDefaults();
//     }
// }