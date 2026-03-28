using Events.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.Queries.GetEventMembers;

public sealed record GetEventMembersQuery(Guid EventId) : IQuery<IReadOnlyList<EventMemberResponse>>;

public sealed record EventMemberResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    IReadOnlyList<string> Permissions,
    EventMemberStatus Status);
