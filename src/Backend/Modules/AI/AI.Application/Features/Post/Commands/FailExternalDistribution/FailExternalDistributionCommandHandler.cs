using Shared.Application.Abstractions;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class FailExternalDistributionCommandHandler
    : ICommandHandler<FailExternalDistributionCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public FailExternalDistributionCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        FailExternalDistributionCommand command,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithDistributionsAsync(command.PostId, cancellationToken);

        if (post is null)
            return Result.Failure(MarketingErrors.Post.NotFound(command.PostId));

        var result = post.FailExternalDistribution(command.Platform, command.ErrorMessage);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result);
    }
}