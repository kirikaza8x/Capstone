using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Domain.Enums;
using Payments.Application.DTOs.Refund;
using Payments.Application.Features.Refunds.Commands.SubmitRefundRequest;
using Payments.Application.Features.Refunds.Queries.GetMyRefundRequests;
using Shared.Api.Results;

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
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SubmitRefundRequestCommand(
                    PaymentTransactionId: request.PaymentTransactionId,
                    Scope: request.Scope,
                    UserReason: request.UserReason,
                    EventSessionId: request.EventSessionId),
                ct);

            return result.ToOk();
        })
        .WithName("SubmitRefundRequest")
        .WithSummary("Submit a refund request for admin review")
        .WithDescription(
            "Scope = SingleItem → EventSessionId required. Refunds one ticket amount.\n" +
            "Scope = FullBatch  → EventSessionId not required. Refunds all non-refunded items.\n" +
            "No money moves until an admin approves.")
        .Produces<RefundRequestDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
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
                new GetMyRefundRequestsQuery(status, page, pageSize), ct);

            return result.ToOk();
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
    Guid? EventSessionId = null
);
