using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventSuspensionExpiredIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; }
    public Guid SuspendedBy { get; }
    public string EventTitle { get; }
    public DateTime SuspendedUntilAtUtc { get; }

    public EventSuspensionExpiredIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid eventId,
        Guid suspendedBy,
        string eventTitle,
        DateTime suspendedUntilAtUtc)
        : base(id, occurredOnUtc)
    {
        EventId = eventId;
        SuspendedBy = suspendedBy;
        EventTitle = eventTitle;
        SuspendedUntilAtUtc = suspendedUntilAtUtc;
    }
}