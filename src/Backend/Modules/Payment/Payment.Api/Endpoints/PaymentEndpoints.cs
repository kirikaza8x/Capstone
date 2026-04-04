using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Application.Features.Vnpay.Commands.InitialPackagePayment;
using Payment.Domain.Enums;
using Payments.Application.Features.Payments.Commands.GetPaymentUrl;
using Payments.Application.Features.Payments.Commands.InitiatePayment;
using Payments.Application.Features.Payments.Queries.GetMyTransactions;
using Shared.Api.Results;

namespace Payments.Api.Features.Payments;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/payments")
            .WithTags("Payments")
            // .RequireAuthorization()
            ;

        // --- Initiate payment ---
        group.MapPost("", async (
            InitiatePaymentRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new InitiatePaymentCommand(
                    OrderId: request.OrderId,
                    Method: request.Method,
                    Description: request.Description),
                ct);

            return result.ToOk();
        })
        .WithName("InitiatePayment")
        .WithSummary("Initiate payment for an order")
        .WithDescription(
            "Method = BatchDirectPay  → returns VNPay redirect URL.\n" +
            "Method = BatchWalletPay  → deducts from wallet immediately.\n" +
            "Order tickets are fetched from Ticketing module automatically.")
        .Produces<InitiatePaymentResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("packages", async (
            InitiatePackagePaymentRequest request,
            ISender sender,
            CancellationToken ct) =>
            {
                var result = await sender.Send(
                    new InitiatePackagePaymentCommand(
                        request.PackageId,
                        request.Method,
                        request.Description),
                    ct);

                return result.ToOk();
            })
        .WithName("InitiatePackagePayment")
        .WithSummary("Initiate payment for an AI package")
        .Produces<InitiatePaymentResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // --- My transaction history ---
        group.MapGet("my", async (
            ISender sender,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(
                new GetMyTransactionsQuery(page, pageSize), ct);

            return result.ToOk();
        })
        .WithName("GetMyTransactions")
        .WithSummary("Get current user's payment transaction history")
        .Produces<GetMyTransactionsResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // --- Reload payment page ---
        group.MapGet("{transactionId:guid}/payment-url", async (
            Guid transactionId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetPaymentUrlCommand(transactionId), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetPaymentUrl")
        .WithSummary("Reload VNPay payment page for a pending transaction")
        .WithDescription(
            "Returns a fresh VNPay redirect URL for an AwaitingGateway transaction. " +
            "Returns 409 if session expired (15 min limit) — initiate a new payment instead. " +
            "Not applicable for BatchWalletPay.")
        .Produces<GetPaymentUrlResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}

public record InitiatePaymentRequest(
    Guid OrderId,
    PaymentType Method,
    string? Description = null
);

public record InitiatePackagePaymentRequest(
    Guid PackageId,
    PaymentType Method,
    string? Description = null);
