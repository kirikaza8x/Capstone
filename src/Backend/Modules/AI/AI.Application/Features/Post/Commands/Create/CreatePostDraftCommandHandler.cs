using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Entities;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class CreatePostDraftCommandHandler
    : ICommandHandler<CreatePostDraftCommand, Guid>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public CreatePostDraftCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreatePostDraftCommand command,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Guard: Tracking token must be unique
        // ─────────────────────────────────────────────────────────────
        var tokenExists = await _postRepository.TrackingTokenExistsAsync(
            command.TrackingToken,
            cancellationToken);

        if (tokenExists)
        {
            return Result.Failure<Guid>(
                MarketingErrors.Post.TrackingTokenAlreadyExists(command.TrackingToken));
        }

        // ─────────────────────────────────────────────────────────────
        // Guard: Validate required fields
        // ─────────────────────────────────────────────────────────────
        if (command.EventId == Guid.Empty)
        {
            return Result.Failure<Guid>(MarketingErrors.Post.EventIdRequired);
        }

        if (command.OrganizerId == Guid.Empty)
        {
            return Result.Failure<Guid>(MarketingErrors.Post.OrganizerIdRequired);
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<Guid>(MarketingErrors.Post.TitleCannotBeEmpty);
        }

        if (string.IsNullOrWhiteSpace(command.Body))
        {
            return Result.Failure<Guid>(MarketingErrors.Post.BodyCannotBeEmpty);
        }

        // ─────────────────────────────────────────────────────────────
        // Create aggregate via factory
        // ─────────────────────────────────────────────────────────────
        PostMarketing post;

        try
        {
            post = PostMarketing.CreateDraft(
                eventId: command.EventId,
                organizerId: command.OrganizerId,
                title: command.Title,
                body: command.Body,
                trackingToken: command.TrackingToken,
                promptUsed: command.PromptUsed,
                aiModel: command.AiModel,
                aiTokensUsed: command.AiTokensUsed);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(
                MarketingErrors.Post.CreateFailed(ex.Message));
        }

        // ─────────────────────────────────────────────────────────────
        // Persist
        // ─────────────────────────────────────────────────────────────
        _postRepository.Add(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(post.Id);
    }
}