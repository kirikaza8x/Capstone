using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventPublishedIntegrationEvent : IntegrationEvent
{
    public Guid AggregateId { get; }

    public EventPublishedIntegrationEvent(Guid id, DateTime occurredOnUtc, Guid aggregateId)
        : base(id, occurredOnUtc)
    {
        AggregateId = aggregateId;
    }
}
