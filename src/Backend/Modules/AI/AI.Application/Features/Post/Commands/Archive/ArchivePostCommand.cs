using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record ArchivePostCommand(
    Guid PostId,
    Guid OrganizerId
) : ICommand;