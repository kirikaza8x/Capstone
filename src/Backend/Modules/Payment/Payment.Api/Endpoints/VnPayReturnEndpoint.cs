using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Application.Features.VnPay.Dtos;

namespace Payments.Api.Features;

public class VnPayReturnEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/payments/vnpay/return", async (
            HttpContext context,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            // Extract all VNPay query parameters into a dictionary
            var queryDictionary = context.Request.Query
                .ToDictionary(q => q.Key, q => q.Value.ToString());

            var command = new VnPayReturnCommand(queryDictionary);

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .WithTags("Payments")
        .WithName("VnPayReturn")
        .WithSummary("Handle VNPay return callback")
        .WithDescription("""
            Handles VNPay's redirect after payment.
            Validates the callback hash securely via Dictionary and processes business logic based on transaction Type.
            """)
        .Produces<VnPayResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
