// ConfirmExternalDistributionCommandHandler.cs
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;
using Microsoft.Extensions.Logging;

namespace Marketing.Application.Posts.Handlers;

public class ConfirmExternalDistributionCommandHandler
    : ICommandHandler<ConfirmExternalDistributionCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmExternalDistributionCommandHandler> _logger;

    public ConfirmExternalDistributionCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork,
        ILogger<ConfirmExternalDistributionCommandHandler> logger)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ConfirmExternalDistributionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Platform == Marketing.Domain.Enums.ExternalPlatform.Threads &&
            !string.IsNullOrWhiteSpace(command.ExternalPostId) &&
            command.ExternalPostId.Contains("REAL_THREADS_MEDIA_ID", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(Error.Validation(
                "Distribution.InvalidExternalPostId",
                "Webhook payload contains placeholder Threads media id. Provide the real media id from Threads API response."));
        }

        const int maxRetries = 5;
        const int delayMs = 500;

        _logger.LogInformation(
            "ConfirmDistribution started: postId={PostId} platform={Platform} url={Url}",
            command.PostId, command.Platform, command.ExternalUrl);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var post = await _postRepository.GetByIdWithDistributionsAsync(
                command.PostId, cancellationToken);

            if (post is null)
            {
                _logger.LogWarning("Post {PostId} not found on attempt {Attempt}",
                    command.PostId, attempt);
                return Result.Failure(MarketingErrors.Post.NotFound(command.PostId));
            }

            var result = post.ConfirmExternalDistribution(
                command.Platform,
                command.ExternalUrl,
                command.ExternalPostId,
                command.PlatformMetadata);

            if (result.IsSuccess)
            {
                _postRepository.Update(post);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "ConfirmDistribution succeeded on attempt {Attempt}: postId={PostId} platform={Platform}",
                    attempt, post.Id, command.Platform);

                return Result.Success(result);
            }

            _logger.LogWarning(
                "ConfirmDistribution attempt {Attempt}/{Max} failed: {Error}",
                attempt, maxRetries, result.Error);

            if (attempt < maxRetries)
            {
                await Task.Delay(delayMs * attempt, cancellationToken);
                continue;
            }

            _logger.LogError(
                "ConfirmDistribution exhausted all {Max} attempts for postId={PostId} platform={Platform}",
                maxRetries, command.PostId, command.Platform);

            return Result.Failure(result.Error);
        }

        return Result.Failure(MarketingErrors.Distribution.NotFound(command.Platform));
    }
}