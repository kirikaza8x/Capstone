using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventCancelledIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; }
    public string? CancellationReason { get; }

    public EventCancelledIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid eventId,
        string? cancellationReason)
        : base(id, occurredOnUtc)
    {
        EventId = eventId;
        CancellationReason = cancellationReason;
    }
}