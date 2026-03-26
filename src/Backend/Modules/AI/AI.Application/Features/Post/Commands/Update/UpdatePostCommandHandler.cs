using Shared.Application.Abstractions.Messaging;
using Shared.Application.Helpers;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Commands;
using AI.Domain.Interfaces.UOW;
using Shared.Application.Abstractions.Authentication;

namespace Marketing.Application.Posts.Handlers;

public class UpdatePostCommandHandler
    : ICommandHandler<UpdatePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAiUnitOfWork _unitOfWork;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        ICurrentUserService currentUserService,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdatePostCommand command,
        CancellationToken cancellationToken)
    {
        Guid requesterId = _currentUserService.GetCurrentUser()?.UserId ?? Guid.Empty;
        bool isAdmin = _currentUserService.GetCurrentUser()?.Roles.Contains("Admin") ?? false;
        var post = await _postRepository.GetByIdAsync(
            command.PostId,
            cancellationToken);

        if (post is null)
        {
            return Result.Failure(
                MarketingErrors.Post.NotFound(command.PostId));
        }

        if (post.OrganizerId != requesterId && !isAdmin)
        {
            return Result.Failure(
                MarketingErrors.Post.NotAuthorized(requesterId));
        }

        string? newSlug = command.Slug;

        if (command.Title is not null &&
            !string.Equals(post.Title, command.Title, StringComparison.Ordinal))
        {
            var baseSlug = SlugHelper.Generate(command.Title);
            newSlug = await GenerateUniqueSlug(baseSlug, cancellationToken);
        }

        // ─────────────────────────────────────────────
        // Domain logic
        // ─────────────────────────────────────────────
        var result = post.Update(
            title: command.Title,
            body: command.Body,
            summary: command.Summary,
            imageUrl: null,
            slug: newSlug,
            promptUsed: command.PromptUsed,
            aiModel: command.AiModel,
            aiTokensUsed: command.AiTokensUsed,
            aiCost: command.AiCost,
            trackingToken: command.TrackingToken
        );

        if (result.IsFailure)
            return result;

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

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
