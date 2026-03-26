using Shared.Application.Abstractions.Messaging;
using Shared.Application.Helpers;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Commands;
using AI.Domain.Interfaces.UOW;

namespace Marketing.Application.Posts.Handlers;

public class UpdatePostCommandHandler
    : ICommandHandler<UpdatePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdatePostCommand command,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(
            command.PostId,
            cancellationToken);

        if (post is null)
        {
            return Result.Failure(
                MarketingErrors.Post.NotFound(command.PostId));
        }

        // Authorization
        if (post.OrganizerId != command.OrganizerId)
        {
            return Result.Failure(
                MarketingErrors.Post.NotAuthorized(command.OrganizerId));
        }

        // ─────────────────────────────────────────────
        // Regenerate slug (if title changed)
        // ─────────────────────────────────────────────
        string? newSlug = null;

        if (!string.Equals(post.Title, command.Title, StringComparison.Ordinal))
        {
            var baseSlug = SlugHelper.Generate(command.Title ?? post.Title);
            newSlug = await GenerateUniqueSlug(baseSlug, cancellationToken);
        }

        // ─────────────────────────────────────────────
        // Domain logic
        // ─────────────────────────────────────────────
        var result = post.Update(
            title: command.Title,
            body: command.Body,
            summary: command.Summary ?? null,   // <-- required
            imageUrl: null,
            slug: newSlug
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