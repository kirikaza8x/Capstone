using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Ticketing.Application.Orders.Queries.ExportOrdersSheet;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Get;

public class ExportOrdersSheetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.ExportOrdersSheet, async (
            [FromQuery] Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new ExportOrdersSheetQuery(eventId);
            var result = await sender.Send(query, cancellationToken);
            return Results.File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("ExportOrdersSheet")
        .WithSummary("Export orders to Excel")
        .WithDescription("Exports all orders of an event into an Excel file")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireRoles(Roles.Organizer);
    }
}
