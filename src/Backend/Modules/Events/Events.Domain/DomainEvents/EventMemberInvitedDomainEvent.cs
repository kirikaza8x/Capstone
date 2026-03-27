using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventMemberInvitedDomainEvent(
    Guid EventMemberId,
    Guid EventId,
    Guid UserId,
    string Email) : DomainEvent;
