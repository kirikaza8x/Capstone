using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventMemberInvitedDomainEvent(
    Guid EventMemberId,
    Guid AggregateEventId,
    Guid UserId,
    string Email) : DomainEvent;
