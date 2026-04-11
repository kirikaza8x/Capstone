using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using AI.Domain.Interfaces.UOW;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Handlers;

public class ArchivePostCommandHandler
    : ICommandHandler<ArchivePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IAiUnitOfWork _unitOfWork;

    public ArchivePostCommandHandler(
        IPostRepository postRepository,
        IAiUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ArchivePostCommand command,
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
        // Authorization: Only organizer can archive their post
        // ─────────────────────────────────────────────────────────────
        if (post.OrganizerId != command.OrganizerId)
        {
            return Result.Failure(
                MarketingErrors.Post.NotAuthorized(command.OrganizerId));
        }

        // ─────────────────────────────────────────────────────────────
        // Execute domain logic
        // ─────────────────────────────────────────────────────────────
        var archiveResult = post.Archive();

        if (archiveResult.IsFailure)
        {
            return Result.Failure(archiveResult.Error);
        }

        // ─────────────────────────────────────────────────────────────
        // Persist
        // ─────────────────────────────────────────────────────────────
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}