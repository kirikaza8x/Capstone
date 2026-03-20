using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventReminderTriggeredIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; }
    public string EventTitle { get; }
    public DateTime EventStartAtUtc { get; }

    public EventReminderTriggeredIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid eventId,
        string eventTitle,
        DateTime eventStartAtUtc)
        : base(id, occurredOnUtc)
    {
        EventId = eventId;
        EventTitle = eventTitle;
        EventStartAtUtc = eventStartAtUtc;
    }
}
