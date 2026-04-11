using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Post.Commands.DeletePostImage;

public sealed record DeletePostImageCommand(Guid PostId) : ICommand<string>;
