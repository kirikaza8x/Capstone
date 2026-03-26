using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Commands;
using Marketing.Api.Features.Posts.Admin;
public class ForceRemovePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/admin/posts/{postId:guid}/force-remove", async (
            Guid postId,
            ForceRemovePostRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ForceRemovePostCommand(
                postId,
                request.AdminId,
                request.Reason
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("ForceRemovePost")
        .WithTags("Posts - Admin")
        .WithSummary("Force-remove a post");
    }
}

public sealed class ForceRemovePostRequestDto
{
    public Guid AdminId { get; init; }
    public string Reason { get; init; } = string.Empty;
}