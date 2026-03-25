using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record UpdatePostCommand(
    Guid PostId,
    Guid OrganizerId,
    string? Title = null,
    string? Body = null
) : ICommand;