using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventCompletedIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; }

    public EventCompletedIntegrationEvent(Guid id, DateTime occurredOnUtc, Guid eventId)
        : base(id, occurredOnUtc)
    {
        EventId = eventId;
    }
}