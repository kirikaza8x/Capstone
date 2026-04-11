using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class QueuePostForDistributionCommandHandler
    : ICommandHandler<QueuePostForDistributionCommand>
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

    // Marketing.Application/Posts/Handlers/QueuePostForDistributionCommandHandler.cs

    public async Task<Result> Handle(
        QueuePostForDistributionCommand command,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Load aggregate
        // ─────────────────────────────────────────────────────────────
        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure(
                MarketingErrors.Post.NotFound(command.PostId));
        }

        // ─────────────────────────────────────────────────────────────
        // Execute domain behavior ← PASS IsRetry HERE
        // ─────────────────────────────────────────────────────────────
        var distributionResult = command.IsRetry
            ? post.QueueForExternalDistribution(command.Platform, allowRetry: true)  // ← Changed
            : post.QueueForExternalDistribution(command.Platform);

        if (distributionResult.IsFailure)
        {
            return Result.Failure(distributionResult.Error);
        }

        // ─────────────────────────────────────────────────────────────
        // Persist changes
        // ─────────────────────────────────────────────────────────────
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(distributionResult);
    }
}