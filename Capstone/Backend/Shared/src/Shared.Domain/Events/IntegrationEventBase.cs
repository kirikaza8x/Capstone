namespace Shared.Contracts.Events;

/// <summary>
/// Base interface for all integration events
/// These are EXTERNAL events published to other services
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Correlation ID to track related events across services
    /// All events in the same business transaction share this
    /// </summary>
    Guid CorrelationId { get; }
    
    /// <summary>
    /// When the event occurred (UTC)
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Which service published this event
    /// Examples: "UserService", "ConfigDbService"
    /// </summary>
    string SourceService { get; }
    
    /// <summary>
    /// Type identifier for the event
    /// Examples: "UserCreated", "JwtConfigurationChanged"
    /// </summary>
    string EventType { get; }
    
    /// <summary>
    /// Event schema version for evolution
    /// </summary>
    int Version => 1;
}