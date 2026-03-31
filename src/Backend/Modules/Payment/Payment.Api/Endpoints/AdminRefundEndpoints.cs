using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.Features.Refunds.Commands.MassRefundBySession;
using Payments.Application.Features.Refunds.Commands.ReviewRefundRequest;
using Payments.Application.Features.Refunds.Queries.GetPendingRefundRequests;
using Payments.Domain.Enums;
using Shared.Api.Results;

namespace Payments.Api.Features.Refunds;

public class AdminRefundEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/refunds")
            .WithTags("Refunds — Admin")
            // .RequireAuthorization("Admin")
            ;

        // --- Review queue ---
        group.MapGet("", async (
            ISender sender,
            CancellationToken ct,
            RefundRequestStatus? status = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(
                new GetPendingRefundRequestsQuery(status, page, pageSize), ct);

            return result.ToOk();
        })
        .WithName("GetRefundQueue")
        .WithSummary("Get refund requests queue")
        .WithDescription(
            "Returns all refund requests. " +
            "Filter by status: Pending, Approved, Rejected.")
        .Produces<GetPendingRefundRequestsResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // --- Approve or reject ---
        group.MapPost("{refundRequestId:guid}/review", async (
            Guid refundRequestId,
            ReviewRefundRequestBody request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new ReviewRefundRequestCommand(
                    RefundRequestId: refundRequestId,
                    Approved: request.Approved,
                    ReviewerNote: request.ReviewerNote),
                ct);

            return result.ToOk();
        })
        .WithName("ReviewRefundRequest")
        .WithSummary("Approve or reject a refund request")
        .WithDescription(
            "Approved = true  → credits user wallet immediately.\n" +
            "Approved = false → records rejection, no money moves.\n" +
            "ReviewerNote required in both cases.")
        .Produces<ReviewRefundRequestResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        // --- Mass refund by session ---
        group.MapPost("sessions/{eventSessionId:guid}/mass-refund", async (
            Guid eventSessionId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new MassRefundBySessionCommand(
                    EventSessionId: eventSessionId,
                    AdminId: Guid.Empty),  // overridden inside handler via ICurrentUserService
                ct);

            return result.ToOk();
        })
        .WithName("MassRefundBySession")
        .WithSummary("Refund all attendees for a cancelled event session")
        .WithDescription(
            "Finds all completed BatchPaymentItems for the given EventSessionId. " +
            "Credits each user's wallet directly — no review step. " +
            "Skips already-refunded items silently. " +
            "Processes in batches of 50 with per-batch transaction safety.")
        .Produces<MassRefundResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record ReviewRefundRequestBody(
    bool Approved,
    string ReviewerNote
);
