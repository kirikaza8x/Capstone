using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.Features.Payments.Commands.VnPayReturn;

namespace Payments.Api.Features.VnPay;

public class VnPayReturnEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/payments/vnpay")
            .WithTags("Payments — VNPay");

        // No auth — VNPay calls this directly as a redirect
        group.MapGet("return", async (
            HttpContext context,
            ISender sender,
            CancellationToken ct) =>
        {
            var queryParams = context.Request.Query
                .ToDictionary(q => q.Key, q => q.Value.ToString());

            var result = await sender.Send(
                new VnPayReturnCommand(queryParams), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("VnPayReturn")
        .WithSummary("Handle VNPay payment return callback")
        .WithDescription("""
            VNPay redirects the user here after payment.
            Validates HMAC hash, updates PaymentTransaction and BatchPaymentItem statuses.
            For WalletTopUp: credits the wallet.
            For BatchDirectPay: marks all items completed.
            """)
        .Produces<VnPayReturnResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}