using Carter;
using Marketing.Application.Posts.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Application.Abstractions.Authentication;
public class GetPostByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/posts/{postId:guid}", async (
            Guid postId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrganizerPostByIdQuery(
                postId
            );

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPostById")
        .WithTags("Posts")
        .WithSummary("Get post details ");
    }
}