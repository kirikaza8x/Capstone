using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.Commands.RemoveEventMember;

public sealed record RemoveEventMemberCommand(Guid EventId, Guid MemberId) : ICommand;
