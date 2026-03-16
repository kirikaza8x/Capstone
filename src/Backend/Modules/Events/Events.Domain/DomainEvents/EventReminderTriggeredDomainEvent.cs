using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventReminderTriggeredDomainEvent(
    Guid EventId,
    string EventTitle,
    DateTime EventStartAtUtc) : DomainEvent;
