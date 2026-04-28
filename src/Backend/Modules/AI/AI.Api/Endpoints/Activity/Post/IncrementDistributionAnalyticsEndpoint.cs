using Shared.Application.Abstractions.Authentication;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
using MediatR;
using Marketing.Domain.Enums;

namespace Marketing.Api.Features.Posts.IncrementDistributionAnalytics;

public class IncrementDistributionAnalyticsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/posts/{postId:guid}/distributions/analytics", async (
            Guid postId,
            IncrementDistributionAnalyticsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new IncrementDistributionAnalyticsCommand(
                PostId: postId,
                Platform: request.Platform,
                DistributionId: request.DistributionId,
                BuyIncrement: request.BuyIncrement,
                ClickIncrement: request.ClickIncrement
            );

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Posts")
        .WithName("IncrementDistributionAnalytics")
        .WithSummary("Increment buy/click analytics for a post distribution");
    }
}

public sealed class IncrementDistributionAnalyticsRequestDto
{
    public ExternalPlatform Platform { get; init; }
    public Guid? DistributionId { get; init; }
    public int BuyIncrement { get; init; }
    public int ClickIncrement { get; init; }
}