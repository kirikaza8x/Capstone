// File: SubmitPostCommand.cs
using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public sealed record SubmitPostCommand(
    Guid PostId,
    Guid OrganizerId
) : ICommand;