namespace Shared.Contracts.Events;

/// <summary>
/// Root metadata present in every message sent across the service bus.
/// Ensures consistent tracing and auditing for both Events and Requests.
/// </summary>
public interface IIntegrationMetadata
{
    /// <summary>
    /// Correlation ID to track a business transaction across multiple services.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// The name of the service that generated this message.
    /// </summary>
    string SourceService { get; }

    /// <summary>
    /// The UTC timestamp when the message was created.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Schema version for message evolution.
    /// </summary>
    int Version { get; }
}

/// <summary>
/// Base interface for all integration events (Pub/Sub).
/// These represent "Facts" that have already occurred.
/// </summary>
public interface IIntegrationEvent : IIntegrationMetadata
{
    /// <summary>
    /// Unique identifier for this specific event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// The type identifier for the event (e.g., "UserCreated").
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base interface for point-to-point communication (Request/Response).
/// Used for direct inquiries between services.
/// </summary>
public interface IIntegrationMessage : IIntegrationMetadata
{
    // Requests and Responses share the core metadata without the overhead of EventType/EventId.
}