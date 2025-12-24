namespace Shared.Application.Abstractions.Authentication;

/// <summary>
/// Extended JWT service interface that supports runtime configuration updates
/// </summary>
public interface IConfigurableJwtService  : IJwtTokenService
{
    /// <summary>
    /// Update JWT configuration at runtime without restarting service
    /// </summary>
    Task UpdateConfigurationAsync(int expiryMinutes, int refreshTokenExpiryDays);

    /// <summary>
    /// Resets the service to hardcoded safety values if all external 
    /// config sources (Redis/ConfigsDB) are unreachable.
    /// </summary>
    public void UseEmergencyDefaults();
}