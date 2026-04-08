using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.RateLimiting;
using Shared.Api.Results;
using Ticketing.Api;
using Ticketing.Application.Orders.Commands.ApplyVoucher;
using Users.PublicApi.Constants;

namespace Ticketing.Presentation.Endpoints.Orders;

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
            {
                return result.ToProblem();
            }

            return Results.Ok(result.Value);
        })
        .RequireRateLimiting(RateLimitPolicies.Order)
        .WithTags(Constants.Tags.Orders)
        .WithName("ApplyVoucherToOrder")
        .WithSummary("Apply a discount voucher to a pending order")
        .WithDescription("Validates and locks the voucher. If an existing voucher is present, it will be swapped out.")
        .Produces<ApplyVoucherResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AttendeeAndOrganizer);
    }
}
