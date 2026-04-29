using System.ComponentModel;
using Carter;
using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Marketing.Api.ExternalDistributions;

public class GetExternalDistributionByPostIdAndPlatformEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/external-distributions/tracking/{postId:guid}", async (
            Guid postId,
            [FromQuery] ExternalPlatform platform,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetExternalDistributionByPostIdAndPlatformQuery(postId, platform);
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetExternalDistributionByPostIdAndPlatform")
        .WithTags("ExternalDistributions")
        .WithDescription("Get a single external distribution by PostId and Platform")
        .Produces<ExternalDistributionDetailDto>(StatusCodes.Status200OK);
    }
}
