// src/Modules/Ticketing/Api/Vouchers/Delete/DeleteVoucherEndpoint.cs

using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Vouchers.Commands.DeleteVoucher;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Vouchers.Delete;

public class DeleteVoucherEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.VoucherById, async (
            [FromRoute] Guid voucherId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new DeleteVoucherCommand(voucherId),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Vouchers)
        .WithName("DeleteVoucher")
        .WithSummary("Delete voucher")
        .WithDescription("Delete voucher. Cannot delete if voucher has been used.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AdminAndOrganizer);
    }
}
