using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents.IntegrationEvents;

public sealed record EventCancelledIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime CancelledAt { get; init; }

    public EventCancelledIntegrationEvent(
        Guid eventId,
        string? cancellationReason,
        DateTime cancelledAt)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        EventId = eventId;
        CancellationReason = cancellationReason;
        CancelledAt = cancelledAt;
    }
}