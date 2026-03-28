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
using Ticketing.Application.Orders.Commands.CheckIn;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Post;

public sealed record CheckInRequest(
    string QrCode,
    Guid EventSessionId);

public class CheckInEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.CheckIn, async (
            Guid eventId,
            [FromBody] CheckInRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CheckInCommand(
                eventId,
                request.QrCode,
                request.EventSessionId);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("CheckIn")
        .WithSummary("Check in a ticket")
        .WithDescription("Validates QR code and marks ticket as used for the given session.")
        .Produces<ApiResult<CheckInResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AttendeeAndOrganizer)
        .RequireEventPermission(EventPermissions.CheckIn);
    }
}
