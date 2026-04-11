using AI.Application.Features.Post.Commands.DeletePostImage;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Application.Abstractions.Storage;
using Users.PublicApi.Constants;

namespace AI.Api.Endpoints.Activity.Post;

public sealed class DeletePostImageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/organizer/posts/{postId:guid}/image", async (
            Guid postId,
            ISender sender,
            IStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeletePostImageCommand(postId), cancellationToken);
            if (result.IsFailure)
                return result.ToProblem();

            try
            {
                await storageService.DeleteAsync(result.Value, cancellationToken);
            }
            catch
            {
            }

            return result.ToOk();
        })
        .WithTags("Posts")
        .WithName("DeletePostImage")
        .WithSummary("Delete post image")
        .Produces<ApiResult<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireRoles(Roles.Organizer);
    }
}
