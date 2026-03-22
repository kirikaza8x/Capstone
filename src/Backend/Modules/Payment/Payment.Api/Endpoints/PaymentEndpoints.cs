using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Domain.Enums;
using Payments.Application.DTOs.Payment;
using Payments.Application.Features.Payments.Commands.InitiatePayment;
using Payments.Application.Features.Payments.Queries.GetMyTransactions;
using Shared.Application.Abstractions.Authentication;

namespace Payments.Api.Features.Payments;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/payments")
            .WithTags("Payments")
            // .RequireAuthorization()
            ;

        // --- Initiate payment (VNPay or wallet, 1 to N events) ---
        group.MapPost("", async (
            InitiatePaymentRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new InitiatePaymentCommand(
                Method: request.Method,
                Items: request.Items,
                Description: request.Description);

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("InitiatePayment")
        .WithSummary("Initiate a payment for one or more events")
        .WithDescription("""
            Method = BatchDirectPay  → returns a VNPay redirect URL.
            Method = BatchWalletPay  → deducts from wallet immediately, returns CompletedAt.
            Items can contain a single event (1-item batch) or multiple events.
            """)
        .Produces<InitiatePaymentResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // --- My transaction history ---
        group.MapGet("my", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(
                new GetMyTransactionsQuery(page, pageSize), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetMyTransactions")
        .WithSummary("Get current user's payment transaction history")
        .Produces<GetMyTransactionsResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record InitiatePaymentRequest(
    PaymentType Method,
    IReadOnlyList<PaymentItemDto> Items,
    string? Description = null
);