using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.DTOs.Refund;
using Payments.Application.Features.Refunds.Commands.SubmitRefundRequest;
using Payments.Application.Features.Refunds.Queries.GetMyRefundRequests;
using Payments.Domain.Enums;
using Shared.Application.Abstractions.Authentication;

namespace Payments.Api.Features.Refunds;

public class UserRefundEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/refunds")
            .WithTags("Refunds — User")
            // .RequireAuthorization()
            ;

        // --- Submit refund request ---
        group.MapPost("", async (
            SubmitRefundRequestBody request,
            // ICurrentUserService currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new SubmitRefundRequestCommand(
                PaymentTransactionId: request.PaymentTransactionId,
                Scope: request.Scope,
                UserReason: request.UserReason,
                EventId: request.EventId);

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("SubmitRefundRequest")
        .WithSummary("Submit a refund request for admin review")
        .WithDescription("""
            Scope = SingleItem → EventId required. Refunds one event's amount.
            Scope = FullBatch  → EventId not required. Refunds all non-refunded items.
            No money moves until an admin approves the request.
            """)
        .Produces<RefundRequestDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        // --- My refund requests ---
        group.MapGet("my", async (
            ISender sender,
            CancellationToken ct,
            RefundRequestStatus? status = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(
                new GetMyRefundRequestsQuery(
                     status, page, pageSize), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetMyRefundRequests")
        .WithSummary("Get current user's refund request history")
        .Produces<GetMyRefundRequestsResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record SubmitRefundRequestBody(
    Guid PaymentTransactionId,
    RefundRequestScope Scope,
    string UserReason,
    Guid? EventId = null
);