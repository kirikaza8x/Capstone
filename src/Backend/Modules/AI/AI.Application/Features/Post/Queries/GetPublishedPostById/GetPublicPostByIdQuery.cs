using Marketing.Application.Posts.Dtos;
using Shared.Application.Abstractions.Messaging;

public record GetPublicPostByIdQuery(Guid PostId)
    : IQuery<PostPublicDto>;