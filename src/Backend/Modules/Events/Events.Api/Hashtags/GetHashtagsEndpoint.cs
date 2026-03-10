using Carter;
using Events.Application.Hashtags.Queries.GetHashtags;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Hashtags;

public class GetHashtagsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Hashtags, async (
            string? name,
            int? take,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetHashtagsQuery(name, take ?? 20), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Hashtags)
        .WithName("GetHashtags")
        .WithSummary("Get all hashtags")
        .WithDescription("Retrieve hashtags. Supports search-as-you-type via `name` query param. Use `take` to limit results.")
        .Produces<IReadOnlyList<HashtagResponse>>(StatusCodes.Status200OK);
    }
}