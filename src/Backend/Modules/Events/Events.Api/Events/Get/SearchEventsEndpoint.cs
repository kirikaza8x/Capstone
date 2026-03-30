using Carter;
using Events.Application.Events.Queries.GetEvents;
using Events.Application.Events.Queries.SearchEvents;
using Events.PublicApi.Constants;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Pagination;

namespace Events.Api.Events.Get;

public class SearchEventsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("events/search", async (
            [AsParameters] SearchEventsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("SearchEvents")
        .WithSummary("Search events by title with fuzzy matching")
        .Produces<ApiResult<PagedResult<EventResponse>>>(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}
