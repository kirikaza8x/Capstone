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
using Ticketing.Application.Orders.Queries.GetTicketsForCheckIn;
using Users.PublicApi.Constants;
using static Ticketing.Api.Constants;

namespace Ticketing.Api.Orders.Get;

public class GetTicketsForCheckInEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Routes.GetTicketsForCheckIn, async (
            Guid eventId,
            [FromQuery] Guid sessionId,
            [FromQuery] string email,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                var error = Shared.Domain.Abstractions.Error.Validation(
                    "Search.EmailRequired",
                    "Email is required to search for tickets.");
                return Shared.Domain.Abstractions.Result.Failure(error).ToProblem();
            }

            var query = new GetTicketsForCheckInQuery(eventId, sessionId, email);

            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Tags.Orders)
        .WithName("GetTicketsForCheckIn")
        .WithSummary("Search tickets by email for manual check-in")
        .WithDescription("Retrieves a list of tickets belonging to the specified email for a given event session.")
        .Produces<ApiResult<IReadOnlyCollection<TicketForCheckInResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AttendeeAndOrganizer)
        .RequireEventPermission(EventPermissions.CheckIn);
    }
}
