using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.Features.Payments.Commands.InitiateTopUp;
using Shared.Api.Results;

namespace Payments.Api.Features.TopUp;

public class TopUpEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/payments/topup")
            .WithTags("Payments — Top Up")
            // .RequireAuthorization()
            ;

        group.MapPost("", async (
            InitiateTopUpRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new InitiateTopUpCommand(
                    Amount: request.Amount,
                    Description: request.Description),
                ct);

            return result.ToOk();
        })
        .WithName("InitiateTopUp")
        .WithSummary("Initiate a VNPay wallet top-up")
        .WithDescription(
            "Creates a WalletTopUp transaction and returns a VNPay redirect URL. " +
            "Wallet is auto-created if the user does not have one yet.")
        .Produces<InitiateTopUpResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record InitiateTopUpRequest(
    decimal Amount,
    string? Description = null
);
