using Shared.Application.Abstractions;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Helpers;
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
    private readonly ITrackingTokenGenerator _tokenGenerator;

    public CreatePostDraftCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork,
        ITrackingTokenGenerator tokenGenerator)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<Guid>> Handle(
        CreatePostDraftCommand command,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Generate Tracking Token (retry if collision)
        // ─────────────────────────────────────────────────────────────
        string trackingToken;
        int retry = 0;

        do
        {
            trackingToken = _tokenGenerator.Generate();

            var exists = await _postRepository.TrackingTokenExistsAsync(
                trackingToken,
                cancellationToken);

            if (!exists) break;

            retry++;
        }
        while (retry < 3);

        if (retry == 3)
        {
            return Result.Failure<Guid>(
                MarketingErrors.Post.TrackingTokenAlreadyExists("Failed to generate unique tracking token after multiple attempts."));
        }

        // ─────────────────────────────────────────────────────────────
        // Generate Slug
        // ─────────────────────────────────────────────────────────────
        var baseSlug = SlugHelper.Generate(command.Title);

        var slug = await GenerateUniqueSlug(baseSlug, cancellationToken);

        // ─────────────────────────────────────────────────────────────
        // Create aggregate
        // ─────────────────────────────────────────────────────────────
        PostMarketing post;

        try
        {
            post = PostMarketing.CreateDraft(
                eventId: command.EventId,
                organizerId: command.OrganizerId,
                title: command.Title,
                body: command.Body,
                slug: slug,
                trackingToken: trackingToken,
                promptUsed: command.PromptUsed,
                aiModel: command.AiModel,
                aiTokensUsed: command.AiTokensUsed
            );
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

    // ─────────────────────────────────────────────────────────────
    // Slug uniqueness helper
    // ─────────────────────────────────────────────────────────────
    private async Task<string> GenerateUniqueSlug(
        string baseSlug,
        CancellationToken cancellationToken)
    {
        var slug = baseSlug;
        int counter = 1;

        while (await _postRepository.SlugExistsAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}