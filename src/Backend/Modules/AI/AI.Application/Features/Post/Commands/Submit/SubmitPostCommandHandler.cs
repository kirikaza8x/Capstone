using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class SubmitPostCommandHandler
    : ICommandHandler<SubmitPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public SubmitPostCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SubmitPostCommand command,
        CancellationToken cancellationToken)
    {
        // Fetch
        var post = await _postRepository.GetByIdAsync(
            command.PostId,
            cancellationToken);

        if (post is null)
        {
            return Result.Failure(
                MarketingErrors.Post.NotFound(command.PostId));
        }

        if (post.OrganizerId != command.OrganizerId)
        {
            return Result.Failure(
                MarketingErrors.Post.NotAuthorized(command.OrganizerId));
        }

        var result = post.Submit();

        if (result.IsFailure)
            return result;

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}