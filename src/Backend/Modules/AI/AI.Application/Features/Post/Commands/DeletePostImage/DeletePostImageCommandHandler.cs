using AI.Domain.Interfaces.UOW;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.PublicApi.Constants;

namespace AI.Application.Features.Post.Commands.DeletePostImage;

public sealed class DeletePostImageCommandHandler(
    IPostRepository postRepository,
    ICurrentUserService currentUserService,
    IAiUnitOfWork unitOfWork) : ICommandHandler<DeletePostImageCommand, string>
{
    public async Task<Result<string>> Handle(DeletePostImageCommand command, CancellationToken cancellationToken)
    {
        var requesterId = currentUserService.UserId;
        var isAdmin = currentUserService.Roles.Contains(Roles.Admin);

        var post = await postRepository.GetByIdAsync(command.PostId, cancellationToken);
        if (post is null)
            return Result.Failure<string>(MarketingErrors.Post.NotFound(command.PostId));

        if (post.OrganizerId != requesterId && !isAdmin)
            return Result.Failure<string>(MarketingErrors.Post.NotAuthorized(requesterId));

        if (string.IsNullOrWhiteSpace(post.ImageUrl))
        {
            return Result.Failure<string>(Error.Validation(
                "PostImage.NotFound",
                "Post has no image to delete."));
        }

        var imageUrlToDelete = post.ImageUrl;

        var updateResult = post.Update(
            title: null,
            body: null,
            summary: null,
            imageUrl: string.Empty,
            slug: null,
            promptUsed: null,
            aiModel: null,
            additionalTokensUsed: null,
            additionalAiCost: null,
            trackingToken: null);

        if (updateResult.IsFailure)
            return Result.Failure<string>(updateResult.Error);

        postRepository.Update(post);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(imageUrlToDelete);
    }
}
