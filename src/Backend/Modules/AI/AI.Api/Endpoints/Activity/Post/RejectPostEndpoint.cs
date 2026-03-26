using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Commands;

public class RejectPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/admin/posts/{postId:guid}/reject", async (
            Guid postId,
            RejectPostRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RejectPostCommand(
                postId,
                request.AdminId,
                request.Reason
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("RejectPost")
        .WithTags("Posts - Admin")
        .WithSummary("Reject a post with feedback");
    }
}

public sealed class RejectPostRequestDto
{
    public Guid AdminId { get; init; }
    public string Reason { get; init; } = string.Empty;
}