using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record ApprovePostCommand(
    Guid PostId,
    Guid AdminId
) : ICommand;