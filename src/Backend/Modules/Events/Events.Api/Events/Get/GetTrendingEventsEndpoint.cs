using Carter;
using Events.Application.Events.Queries.GetEvents;
using Events.Application.Events.Queries.GetTrendingEvents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Pagination;

namespace Events.Api.Events.Get;

public class GetTrendingEventsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Trending, async (
            [AsParameters] GetTrendingEventsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Search)
        .WithName("GetTrendingEvents")
        .WithSummary("Get trending events")
        .WithDescription("Returns published events ranked by ticket sales within the specified number of days.")
        .Produces<ApiResult<PagedResult<EventResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
