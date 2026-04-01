using Carter;
using Events.PublicApi.Constants;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Api.Extensions;
using Ticketing.Application.Reports.GetEventTicketSales;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Reports;

public class GetEventTicketSalesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.EventTicketSales, async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetEventTicketSalesQuery(eventId);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Reports)
        .WithName("GetEventTicketSales")
        .WithSummary("Get ticket sales and check-in breakdown for an event")
        .WithDescription("Retrieves total tickets sold, check-in rates, revenue, and breakdown by ticket type for organizers.")
        .Produces<ApiResult<TicketSalesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireRoles(Roles.AttendeeAndOrganizer)
        .RequireEventPermission(EventPermissions.ViewReports);
    }
}
