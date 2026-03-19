using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Payment.Application.Features.VnPay.Dtos;
using Payments.Application.Features.Commands.InitiatePayment;

namespace Payments.Api.Features
{
    public class InitiatePaymentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/payments/initiate", async (
                InitiatePaymentCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);
                return result.ToOk();
            })
            .WithTags("Payments")
            .WithName("InitiatePayment")
            .WithSummary("Initiate a VNPay payment")
            .WithDescription("""
                Creates a VNPay payment URL for the given order.
                Returns a secure redirect URL that the client should use to complete payment.
                """)
            .Produces<InitiatePaymentResponseDto>(StatusCodes.Status200OK);
        }
    }
}
