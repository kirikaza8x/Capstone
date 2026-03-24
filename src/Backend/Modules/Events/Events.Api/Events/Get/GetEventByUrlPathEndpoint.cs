using Carter;
using Events.Application.Events.Queries.GetEventByUrlPath;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;


namespace Events.Api.Events.Get;

public class GetEventByUrlPathEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.EventByUrlPath, async (
            [FromRoute] string urlPath,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetEventByUrlPathQuery(urlPath),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("GetEventByUrlPath")
        .WithSummary("Get event by url path")
        .WithDescription("Retrieve event detail by its url path.")
        .Produces<ApiResult<GetEventByUrlPathResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
