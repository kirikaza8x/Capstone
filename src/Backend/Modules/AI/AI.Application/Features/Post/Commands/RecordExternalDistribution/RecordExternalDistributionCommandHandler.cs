using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Commands;
using AI.Domain.Interfaces.UOW;

namespace Marketing.Application.Posts.Handlers;

public class RecordExternalDistributionCommandHandler
    : ICommandHandler<RecordExternalDistributionCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public RecordExternalDistributionCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RecordExternalDistributionCommand command,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Fetch aggregate
        // ─────────────────────────────────────────────────────────────
        var post = await _postRepository.GetByIdAsync(
            command.PostId,
            cancellationToken);

        if (post == null)
        {
            return Result.Failure(
                MarketingErrors.Post.NotFound(command.PostId));
        }

        // ─────────────────────────────────────────────────────────────
        // Execute domain logic (system callback - no auth check)
        // Note: API key auth should be handled at endpoint layer
        // ─────────────────────────────────────────────────────────────
        var recordResult = post.RecordExternalDistribution(command.ExternalUrl);

        if (recordResult.IsFailure)
        {
            return Result.Failure(recordResult.Error);
        }

        // ─────────────────────────────────────────────────────────────
        // Persist
        // ─────────────────────────────────────────────────────────────
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}