using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class QueuePostForDistributionCommandHandler
    : ICommandHandler<QueuePostForDistributionCommand, Result>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public QueuePostForDistributionCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Result>> Handle(
        QueuePostForDistributionCommand command,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Load aggregate
        // ─────────────────────────────────────────────────────────────
        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure<Result>(
                MarketingErrors.Post.NotFound(command.PostId));
        }

        // ─────────────────────────────────────────────────────────────
        // Execute domain behavior
        // ─────────────────────────────────────────────────────────────
        var distributionResult = post.QueueForExternalDistribution(command.Platform);

        if (distributionResult.IsFailure)
        {
            return Result.Failure<Result>(distributionResult.Error);
        }

        // ─────────────────────────────────────────────────────────────
        // Persist changes (triggers domain event for n8n handler later)
        // ─────────────────────────────────────────────────────────────
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(distributionResult);
    }
}