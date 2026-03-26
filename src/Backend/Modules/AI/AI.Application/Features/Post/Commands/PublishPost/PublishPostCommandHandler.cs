using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Commands;
using AI.Domain.Interfaces.UOW;

namespace Marketing.Application.Posts.Handlers;

public class PublishPostCommandHandler
    : ICommandHandler<PublishPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public PublishPostCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        PublishPostCommand command,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(
            command.PostId,
            cancellationToken);

        if (post is null)
        {
            return Result.Failure(
                MarketingErrors.Post.NotFound(command.PostId));
        }

        // Authorization
        if (post.OrganizerId != command.OrganizerId)
        {
            return Result.Failure(
                MarketingErrors.Post.NotAuthorized(command.OrganizerId));
        }

        var result = post.Publish();

        if (result.IsFailure)
            return result;

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}