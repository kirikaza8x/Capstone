using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Application.Features.VnPay.Dtos;
using Payments.Application.Features.Commands.RefundByEvent;
using Payments.Application.Features.Commands.WalletPay;
using Shared.Application.Abstractions.Authentication;

namespace Payments.Api.Features;

public class WalletPayEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/payments/wallet-pay", async (
            WalletPayRequest request,
            // ICurrentUserService currentUser,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new WalletPayCommand(
                // UserId: currentUser.UserId,
                EventId: request.EventId,
                Amount: request.Amount,
                Description: request.Description
            );

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .RequireAuthorization()
        .WithTags("Payments")
        .WithName("WalletPay")
        .WithSummary("Pay for an event using wallet balance")
        .Produces<WalletPayResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);


    }
}

public record WalletPayRequest(
    Guid EventId,
    decimal Amount,
    string? Description = null
);
