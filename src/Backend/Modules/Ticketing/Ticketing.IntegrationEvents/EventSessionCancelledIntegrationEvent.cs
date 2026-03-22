using Shared.Application.Abstractions.EventBus;

namespace Ticketing.IntegrationEvents;

public sealed record EventSessionCancelledIntegrationEvent : IntegrationEvent
{
    public Guid EventSessionId { get; init; }
    public DateTime CancelledAtUtc { get; init; }

    public EventSessionCancelledIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid eventSessionId,
        DateTime cancelledAtUtc)
        : base(id, occurredOnUtc)
    {
        EventSessionId = eventSessionId;
        CancelledAtUtc = cancelledAtUtc;
    }
}
