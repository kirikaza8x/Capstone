namespace Shared.Contracts.Events.Configs;

using Shared.Contracts.Events;

/// <summary>
/// INTEGRATION EVENT - Published by ConfigDbService
/// Raised when: JWT configuration changes
/// Consumers: UserService (hot-reload JWT settings)
/// Benefit: No service restart needed when config changes!
/// </summary>
public record JwtConfigurationChangedEvent : IntegrationEventBase
{
    public override string EventType => nameof(JwtConfigurationChangedEvent);
    
    public string ConfigKey { get; init; } = "JWT";
    public int ExpiryMinutes { get; init; }
    public int RefreshTokenExpiryDays { get; init; }
    public DateTime ChangedAt { get; init; }
    public string ChangedBy { get; init; } = "System";
    
    /// <summary>
    /// If true, services should restart to apply changes
    /// (e.g., secret key rotation)
    /// </summary>
    public bool RequiresServiceRestart { get; init; }
}