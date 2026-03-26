using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Commands;

namespace Marketing.Api.Features.Posts.Admin;

public class ApprovePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/admin/posts/{postId:guid}/approve", async (
            Guid postId,
            ApprovePostRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ApprovePostCommand(postId, request.AdminId);

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("ApprovePost")
        .WithTags("Posts - Admin")
        .WithSummary("Approve a post for publishing");
    }
}

public sealed class ApprovePostRequestDto
{
    public Guid AdminId { get; init; }
}