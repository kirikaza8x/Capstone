namespace Shared.Contracts.Events.Configs;

/// <summary>
/// Request sent by a service at startup to retrieve JWT configurations.
/// </summary>
public interface IGetJwtConfigurationRequest : IIntegrationMessage 
{ 
}

/// <summary>
/// Response containing the active JWT configuration details.
/// </summary>
public interface IJwtConfigurationResponse : IIntegrationMessage
{
    /// <summary>
    /// Access token expiration time in minutes.
    /// </summary>
    int ExpiryMinutes { get; }

    /// <summary>
    /// Refresh token validity duration in days.
    /// </summary>
    int RefreshTokenExpiryDays { get; }
}