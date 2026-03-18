using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventSuspendedIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; }
    public Guid OrganizerId { get; }
    public Guid SuspendedBy { get; }
    public string EventTitle { get; }
    public string SuspensionReason { get; }
    public DateTime SuspendedUntilAtUtc { get; }

    public EventSuspendedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid eventId,
        Guid organizerId,
        Guid suspendedBy,
        string eventTitle,
        string suspensionReason,
        DateTime suspendedUntilAtUtc)
        : base(id, occurredOnUtc)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        SuspendedBy = suspendedBy;
        EventTitle = eventTitle;
        SuspensionReason = suspensionReason;
        SuspendedUntilAtUtc = suspendedUntilAtUtc;
    }
}