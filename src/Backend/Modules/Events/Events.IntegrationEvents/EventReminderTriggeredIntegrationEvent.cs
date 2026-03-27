using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventReminderTriggeredIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; }
    public Guid OrganizerId { get; }
    public string EventTitle { get; }
    public DateTime EventStartAtUtc { get; }

    public EventReminderTriggeredIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid eventId,
        Guid organizerId,
        string eventTitle,
        DateTime eventStartAtUtc)
        : base(id, occurredOnUtc)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        EventTitle = eventTitle;
        EventStartAtUtc = eventStartAtUtc;
    }
}
