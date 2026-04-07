using AI.Domain.Interfaces.UOW;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.PublicApi.Constants;

namespace AI.Application.Features.Post.Commands.UpdatePostImage;

public sealed class UpdatePostImageCommandHandler(
    IPostRepository postRepository,
    ICurrentUserService currentUserService,
    IAiUnitOfWork unitOfWork) : ICommandHandler<UpdatePostImageCommand, string>
{
    public async Task<Result<string>> Handle(
        UpdatePostImageCommand command,
        CancellationToken cancellationToken)
    {
        var requesterId = currentUserService.UserId;
        var isAdmin = currentUserService.Roles.Contains(Roles.Admin);

        var post = await postRepository.GetByIdAsync(command.PostId, cancellationToken);
        if (post is null)
            return Result.Failure<string>(MarketingErrors.Post.NotFound(command.PostId));

        if (post.OrganizerId != requesterId && !isAdmin)
            return Result.Failure<string>(MarketingErrors.Post.NotAuthorized(requesterId));

        var updateResult = post.Update(
            title: null,
            body: null,
            summary: null,
            imageUrl: command.ImageUrl,
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

        return Result.Success(command.ImageUrl);
    }
}
