namespace Shared.Contracts.Events;

/// <summary>
/// Base class for all integration events
/// Provides common properties implementation
/// </summary>
public abstract record IntegrationEventBase : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string SourceService { get; init; } = string.Empty;
    public abstract string EventType { get; }
    public virtual int Version => 1;
    
    /// <summary>
    /// Optional metadata for tracing, debugging, auditing
    /// Example: {"RequestId": "123", "UserId": "456"}
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}