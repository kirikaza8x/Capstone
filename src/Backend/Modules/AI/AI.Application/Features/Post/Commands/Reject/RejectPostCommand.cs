using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record RejectPostCommand(
    Guid PostId,
    Guid AdminId,
    string Reason
) : ICommand;