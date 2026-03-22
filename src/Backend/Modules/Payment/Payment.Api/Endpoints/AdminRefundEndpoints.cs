using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.Features.Refunds.Commands.MassRefundByEvent;
using Payments.Application.Features.Refunds.Commands.ReviewRefundRequest;
using Payments.Application.Features.Refunds.Queries.GetPendingRefundRequests;
using Payments.Domain.Enums;
using Shared.Application.Abstractions.Authentication;

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

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetRefundQueue")
        .WithSummary("Get refund requests queue")
        .WithDescription("Returns all refund requests. Filter by status: Pending, Approved, Rejected.")
        .Produces<GetPendingRefundRequestsResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // --- Approve or reject ---
        group.MapPost("{refundRequestId:guid}/review", async (
            Guid refundRequestId,
            ReviewRefundRequestBody request,
            ICurrentUserService currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new ReviewRefundRequestCommand(
                RefundRequestId: refundRequestId,
                AdminId: currentUser.UserId,
                Approved: request.Approved,
                ReviewerNote: request.ReviewerNote);

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("ReviewRefundRequest")
        .WithSummary("Approve or reject a refund request")
        .WithDescription("""
            Approved = true  → credits the user's wallet immediately.
            Approved = false → records rejection reason, no money moves.
            ReviewerNote is required in both cases.
            """)
        .Produces<ReviewRefundRequestResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        // --- Mass refund (event cancellation) ---
        group.MapPost("events/{eventId:guid}/mass-refund", async (
            Guid eventId,
            ICurrentUserService currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new MassRefundByEventCommand(eventId, currentUser.UserId), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("MassRefundByEvent")
        .WithSummary("Refund all attendees for a cancelled event")
        .WithDescription("""
            Finds all completed BatchPaymentItems for the given EventId across all users.
            Credits each user's wallet directly — no review step.
            Skips already-refunded items silently.
            Processes in batches of 50 with per-batch transaction safety.
            """)
        .Produces<MassRefundResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record ReviewRefundRequestBody(
    bool Approved,
    string ReviewerNote
);