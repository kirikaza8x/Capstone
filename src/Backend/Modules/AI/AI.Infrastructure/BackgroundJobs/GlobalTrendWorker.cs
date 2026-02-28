using AI.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AI.Infrastructure.BackgroundJobs
{
    public class GlobalTrendWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GlobalTrendWorker> _logger;
        
        // Configuration: Run every 1 hour
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public GlobalTrendWorker(IServiceProvider serviceProvider, ILogger<GlobalTrendWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Global Trend Worker Started. Running initial calculation...");

            try
            {
                // 1. Run immediately on startup (so you don't wait 1 hour to see if it works)
                await RunJobAsync(stoppingToken);

                // 2. Start the loop
                using PeriodicTimer timer = new PeriodicTimer(_period);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunJobAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Service is stopping, ignore this exception
                _logger.LogInformation("Global Trend Worker is stopping (cancellation requested).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Global Trend Worker encountered an unexpected error.");
                throw;
            }
        }

        private async Task RunJobAsync(CancellationToken stoppingToken)
        {
            try 
            {
                _logger.LogInformation("Executing Scheduled Global Trend Update...");

                // CRITICAL: Create a scope because GlobalTrendService uses DbContext (which is Scoped)
                using (var scope = _serviceProvider.CreateScope())
                {
                    var trendService = scope.ServiceProvider.GetRequiredService<IGlobalTrendService>();                    
                    await trendService.UpdateGlobalTrendsAsync();
                }

                _logger.LogInformation("Global Trend Update Completed Successfully.");
            }
            catch (Exception ex)
            {
                // Never let the background job crash the app!
                _logger.LogError(ex, "CRITICAL ERROR: Global Trend calculation failed.");
            }
        }
    }
}