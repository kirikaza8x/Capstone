using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record PublishPostCommand(
    Guid PostId,
    Guid OrganizerId
) : ICommand;