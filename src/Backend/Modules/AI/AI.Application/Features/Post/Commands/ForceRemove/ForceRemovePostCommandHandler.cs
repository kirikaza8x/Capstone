using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Commands;
using AI.Domain.Interfaces.UOW;

namespace Marketing.Application.Posts.Handlers;

public class ForceRemovePostCommandHandler
    : ICommandHandler<ForceRemovePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public ForceRemovePostCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ForceRemovePostCommand command,
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

        var result = post.AdminRemove(
            command.AdminId,
            command.Reason);

        if (result.IsFailure)
            return result;

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}