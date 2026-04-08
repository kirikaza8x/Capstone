using Carter;
using Events.PublicApi.Constants;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Api.Extensions;
using Ticketing.Application.Orders.Commands.ManualCheckIn;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Post;

public sealed record ManualCheckInRequest(Guid EventSessionId, List<Guid> OrderTicketIds);

public class ManualCheckInEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/ticketing/events/{eventId:guid}/check-in/manual", async (
            Guid eventId,
            [FromBody] ManualCheckInRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ManualCheckInCommand(
                eventId,
                request.EventSessionId,
                request.OrderTicketIds);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblem();
            }

            return result.ToOk("Check-in successfully");
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("ManualCheckIn")
        .WithSummary("Manually check in tickets")
        .WithDescription("Marks selected tickets as used for the given session and event.")
        .Produces<ApiResult<int>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AttendeeAndOrganizer)
        .RequireEventPermission(EventPermissions.CheckIn);
    }
}
