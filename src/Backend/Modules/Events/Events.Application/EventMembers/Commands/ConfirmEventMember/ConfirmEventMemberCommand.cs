using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.Commands.ConfirmEventMember;
public sealed record ConfirmEventMemberCommand(
    Guid EventId,
    Guid MemberId) : ICommand;
