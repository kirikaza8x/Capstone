using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Orders.Commands.ApplyVoucher;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Post;

public sealed record ApplyVoucherRequest(string CouponCode);

public class ApplyVoucherEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.ApplyVoucher, async (
            Guid orderId,
            [FromBody] ApplyVoucherRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ApplyVoucherCommand(orderId, request.CouponCode),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("ApplyVoucher")
        .WithSummary("Apply voucher to order")
        .WithDescription("Applies a voucher discount to a pending order. Replaces existing voucher if any.")
        .Produces<ApplyVoucherResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AttendeeAndOrganizer);
    }
}
