using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventReminderTriggeredDomainEvent(
    Guid AggregateEventId,
    Guid OrganizerId,
    string EventTitle,
    DateTime EventStartAtUtc) : DomainEvent;
