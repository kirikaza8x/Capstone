using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Commands;
using AI.Domain.Interfaces.UOW;

namespace Marketing.Application.Posts.Handlers;

public class PublishPostCommandHandler
    : ICommandHandler<PublishPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public PublishPostCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        PublishPostCommand command,
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
        // Authorization: Only organizer can publish their post
        // ─────────────────────────────────────────────────────────────
        if (post.OrganizerId != command.OrganizerId)
        {
            return Result.Failure(
                MarketingErrors.Post.NotAuthorized(command.OrganizerId));
        }

        // ─────────────────────────────────────────────────────────────
        // Execute domain logic
        // ─────────────────────────────────────────────────────────────
        try
        {
            var publishResult = post.Publish();

            if (publishResult.IsFailure)
            {
                return Result.Failure(publishResult.Error);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                MarketingErrors.Post.PublishFailed(ex.Message));
        }

        // ─────────────────────────────────────────────────────────────
        // Persist
        // ─────────────────────────────────────────────────────────────
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}