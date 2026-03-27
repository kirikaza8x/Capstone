using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Vouchers.Commands.UpdateVoucher;
using Ticketing.Domain.Enums;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Vouchers;

public sealed record UpdateVoucherRequest(
    string Name,
    string? Description,
    string CouponCode,
    VoucherType Type,
    decimal Value,
    int MaxUse,
    DateTime StartDate,
    DateTime EndDate);

public class UpdateVoucherEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.VoucherById, async (
            [FromRoute] Guid voucherId,
            [FromBody] UpdateVoucherRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateVoucherCommand(
                voucherId,
                request.Name,
                request.Description,
                request.CouponCode,
                request.Type,
                request.Value,
                request.MaxUse,
                request.StartDate,
                request.EndDate),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Vouchers)
        .WithName("UpdateVoucher")
        .WithSummary("Update voucher")
        .WithDescription("Update voucher. Cannot update if voucher has been used.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AdminAndOrganizer);
    }
}
