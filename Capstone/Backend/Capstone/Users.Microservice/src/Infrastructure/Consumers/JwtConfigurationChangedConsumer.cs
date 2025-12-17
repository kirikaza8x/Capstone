using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Configs;
using Shared.Application.Abstractions.Authentication;

namespace Users.Infrastructure.Consumers;

/// <summary>
/// Listens to JWT configuration changes from ConfigDbService
/// Hot-reloads JWT settings in UserService at runtime
/// </summary>
public class JwtConfigurationChangedConsumer : IConsumer<JwtConfigurationChangedEvent>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<JwtConfigurationChangedConsumer> _logger;

    public JwtConfigurationChangedConsumer(
        IJwtTokenService jwtTokenService,
        ILogger<JwtConfigurationChangedConsumer> logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JwtConfigurationChangedEvent> context)
    {
        var evt = context.Message;
        
        _logger.LogInformation(
            "Received JWT configuration change from {Source}: ExpiryMinutes={Expiry}, RefreshDays={Refresh}",
            evt.SourceService,
            evt.ExpiryMinutes,
            evt.RefreshTokenExpiryDays
        );

        try
        {
            // Hot-reload JWT configuration
            if (_jwtTokenService is IConfigurableJwtService configurableService)
            {
                await configurableService.UpdateConfigurationAsync(
                    evt.ExpiryMinutes,
                    evt.RefreshTokenExpiryDays
                );
                
                _logger.LogInformation("JWT configuration updated successfully");
            }
            else
            {
                _logger.LogWarning(
                    "JWT service does not support hot-reload. Service restart required.");
            }

            // Important: MassTransit expects Task completion
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update JWT configuration");
            
            // Throwing here will trigger retry policy
            // Message will be retried based on UseMessageRetry configuration
            throw;
        }
    }
}