using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.Features.Payments.Commands.InitiateTopUp;
using Shared.Application.Abstractions.Authentication;

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
            ICurrentUserService currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new InitiateTopUpCommand(
                UserId: currentUser.UserId,
                IpAddress: currentUser.IpAddress ?? "127.0.0.1",
                Amount: request.Amount,
                Description: request.Description);

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("InitiateTopUp")
        .WithSummary("Initiate a VNPay wallet top-up")
        .WithDescription("Creates a PaymentTransaction and returns a VNPay redirect URL.")
        .Produces<InitiateTopUpResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record InitiateTopUpRequest(
    decimal Amount,
    string? Description = null
);