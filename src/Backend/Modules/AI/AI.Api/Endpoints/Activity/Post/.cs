using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
using Shared.Application.Abstractions.Authentication;
using Marketing.Domain.Enums;

namespace Marketing.Api.Features.Posts.QueuePostForDistribution;

public class QueuePostForDistributionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/posts/{postId:guid}/distribute", async (
            Guid postId,
            QueuePostForDistributionRequestDto request,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new QueuePostForDistributionCommand(
                PostId: postId,
                Platform: request.Platform
            );

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .RequireAuthorization()
        .WithTags("Posts")
        .WithName("QueuePostForDistribution")
        .WithSummary("Manually trigger distribution of a published post to an external platform (e.g., Facebook)");
    }
}

public sealed class QueuePostForDistributionRequestDto
{
    /// <summary>
    /// Target platform for distribution (Facebook, LinkedIn, etc.)
    /// </summary>
    public ExternalPlatform Platform { get; init; }
}