using Carter;
using Events.Application.Hashtags.Queries.GetHashtagById;
using Events.Application.Hashtags.Queries.GetHashtags;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Hashtags;

public class GetHashtagByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.HashtagById, async (
            [FromRoute] int hashtagId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetHashtagByIdQuery(hashtagId), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Hashtags)
        .WithName("GetHashtagById")
        .WithSummary("Get hashtag by ID")
        .WithDescription("Retrieve a single hashtag by its ID.")
        .Produces<ApiResult<HashtagResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}