using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.Commands.AddEventMember;

public sealed record AddEventMemberCommand(
    Guid EventId,
    string Email,
    List<string> Permissions) : ICommand<Guid>;