using Shared.Application.Abstractions;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class ConfirmExternalDistributionCommandHandler
    : ICommandHandler<ConfirmExternalDistributionCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public ConfirmExternalDistributionCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ConfirmExternalDistributionCommand command,
        CancellationToken cancellationToken)
    {
        // Load aggregate WITH distributions
        var post = await _postRepository.GetByIdWithDistributionsAsync(command.PostId, cancellationToken);

        if (post is null)
            return Result.Failure(MarketingErrors.Post.NotFound(command.PostId));

        // Execute domain behavior
        var result = post.ConfirmExternalDistribution(
            command.Platform,
            command.ExternalUrl,
            command.ExternalPostId,
            command.PlatformMetadata);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        // Persist
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result);
    }
}