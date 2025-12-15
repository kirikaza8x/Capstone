namespace Shared.Domain.Events;

/// <summary>
/// Marker interface for events published across microservices
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    Guid CorrelationId { get; }
    DateTime OccurredAt { get; }
    string SourceService { get; }
    string EventType { get; }
}