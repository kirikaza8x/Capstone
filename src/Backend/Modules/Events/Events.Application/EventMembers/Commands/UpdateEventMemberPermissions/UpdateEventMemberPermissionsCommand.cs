using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.Commands.UpdateEventMemberPermissions;

public sealed record UpdateEventMemberPermissionsCommand(
    Guid EventId,
    Guid MemberId,
    List<string> Permissions) : ICommand;