using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record ForceRemovePostCommand(
    Guid PostId,
    Guid AdminId,
    string Reason
) : ICommand;