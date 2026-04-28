using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Commands;
using AI.Domain.Interfaces.UOW;
using Shared.Application.Abstractions.Authentication;

namespace Marketing.Application.Posts.Handlers;

public class IncrementDistributionAnalyticsCommandHandler
    : ICommandHandler<IncrementDistributionAnalyticsCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public IncrementDistributionAnalyticsCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        IncrementDistributionAnalyticsCommand command,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithDistributionsAsync(command.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(MarketingErrors.Post.NotFound(command.PostId));

        var result = post.IncrementDistributionAnalytics(
            platform: command.Platform,
            distributionId: command.DistributionId,
            buyIncrement: command.BuyIncrement,
            clickIncrement: command.ClickIncrement);

        if (result.IsFailure)
            return result;

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}