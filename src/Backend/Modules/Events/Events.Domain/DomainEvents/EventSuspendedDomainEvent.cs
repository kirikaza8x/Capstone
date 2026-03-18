using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventSuspendedDomainEvent(
    Guid EventId,
    Guid OrganizerId,
    Guid SuspendedBy,
    string EventTitle,
    string SuspensionReason,
    DateTime SuspendedUntilAtUtc) : DomainEvent;