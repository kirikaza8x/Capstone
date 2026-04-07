
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Post.Commands.UpdatePostImage;

public sealed record UpdatePostImageCommand(
    Guid PostId,
    string ImageUrl) : ICommand<string>;
