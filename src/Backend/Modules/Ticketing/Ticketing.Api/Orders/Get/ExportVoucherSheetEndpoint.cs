using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Ticketing.Application.Orders.Queries.ExportVoucherSheet;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Get;

public class ExportVoucherSheetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.ExportVouchersSheet, async (
            [FromQuery] Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new ExportVoucherSheetQuery(eventId);
            var result = await sender.Send(query, cancellationToken);
            return Results.File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "vouchers.xlsx");
        })
        .WithTags(Constants.Tags.Vouchers)
        .WithName("ExportVoucherSheet")
        .WithSummary("Export vouchers to Excel")
        .WithDescription("Exports all vouchers into an Excel file")
        .RequireRoles(Roles.Organizer);
    }
}
