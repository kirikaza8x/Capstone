using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class ApprovePostCommandHandler
    : ICommandHandler<ApprovePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public ApprovePostCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ApprovePostCommand command,
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

        // Domain logic (admin action)
        var result = post.Approve(command.AdminId);

        if (result.IsFailure)
            return result;

        // Persist
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}