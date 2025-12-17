namespace Shared.Application.Abstractions.Authentication;

/// <summary>
/// Extended JWT service interface that supports runtime configuration updates
/// </summary>
public interface IConfigurableJwtService
{
    /// <summary>
    /// Update JWT configuration at runtime without restarting service
    /// </summary>
    Task UpdateConfigurationAsync(int expiryMinutes, int refreshTokenExpiryDays);
}