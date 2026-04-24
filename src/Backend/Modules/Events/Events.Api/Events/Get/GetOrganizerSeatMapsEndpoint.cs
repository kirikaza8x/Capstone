using Carter;
using Events.Application.Events.Queries.GetOrganizerSeatMaps;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Get;

public class GetOrganizerSeatMapsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"{Constants.Routes.OrganizerEvents}/me/seatmaps", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOrganizerSeatMapsQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.EventForOrganizer)
        .WithName("GetOrganizerSeatMaps")
        .WithSummary("Get seatmaps from organizer's past events")
        .WithDescription("Retrieve all events of the current organizer that have a seatmap (spec), returning event id, title, and spec for reuse.")
        .Produces<ApiResult<IReadOnlyList<OrganizerSeatMapResponse>>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Organizer);
    }
}
