using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.DTOs.Wallet;
using Payments.Application.Features.Payments.Queries.GetMyWallet;
using Shared.Application.Abstractions.Authentication;

namespace Payments.Api.Features.Wallet;

public class WalletEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/wallet")
            .WithTags("Wallet")
            // .RequireAuthorization()
            ;

        group.MapGet("", async (
            ISender sender,
            CancellationToken ct,
            int transactionLimit = 10) =>
        {
            var result = await sender.Send(
                new GetMyWalletQuery(transactionLimit), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetMyWallet")
        .WithSummary("Get current user's wallet balance and recent transactions")
        .Produces<WalletWithTransactionsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}