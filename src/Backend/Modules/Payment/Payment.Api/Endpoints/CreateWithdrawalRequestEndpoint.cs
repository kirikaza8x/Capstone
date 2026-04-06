using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Domain.Enums;
using Payments.Application.Features.WithdrawalRequests.Commands;
using Payments.Application.Features.WithdrawalRequests.Dtos;
using Payments.Application.Features.WithdrawalRequests.Queries;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using System.ComponentModel;

namespace Payments.Api.WithdrawalRequests;

// ══════════════════════════════════════════════════════════════════════════════
// USER — Create
// ══════════════════════════════════════════════════════════════════════════════

public class CreateWithdrawalRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/withdrawal-requests", async (
            CreateWithdrawalRequestDto body,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateWithdrawalRequestCommand(
                BankAccountNumber: body.BankAccountNumber,
                BankName: body.BankName,
                Amount: body.Amount,
                Notes: body.Notes,
                ReceiverName: body.ReceiverName);

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization()
        .WithName("CreateWithdrawalRequest")
        .WithTags("Withdrawal Requests")
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
    }
}

public sealed record CreateWithdrawalRequestDto(
    string BankAccountNumber,
    string BankName,
    decimal Amount,
    string? Notes,
    string? ReceiverName);

// ══════════════════════════════════════════════════════════════════════════════
// USER — Cancel
// ══════════════════════════════════════════════════════════════════════════════

public class CancelWithdrawalRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/withdrawal-requests/{id:guid}/cancel", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CancelWithdrawalRequestCommand(RequestId: id);
            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization()
        .WithName("CancelWithdrawalRequest")
        .WithTags("Withdrawal Requests")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// USER — My list
// ══════════════════════════════════════════════════════════════════════════════

public class GetMyWithdrawalRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/withdrawal-requests/me", async (
            [AsParameters] GetMyWithdrawalRequestsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyWithdrawalRequestsQuery
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder?.ToLower() == "asc"
                                ? SortOrder.Ascending
                                : SortOrder.Descending,
                Status = request.Status
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization()
        .WithName("GetMyWithdrawalRequests")
        .WithTags("Withdrawal Requests")
        .Produces<PagedResult<WithdrawalRequestListItemDto>>(StatusCodes.Status200OK);
    }
}

public sealed record GetMyWithdrawalRequestsRequestDto
{
    [DefaultValue(1)] public int PageNumber { get; init; } = 1;
    [DefaultValue(10)] public int PageSize { get; init; } = 10;

    [DefaultValue("CreatedAt")] public string SortColumn { get; init; } = "CreatedAt";
    [DefaultValue("desc")] public string SortOrder { get; init; } = "desc";

    public WithdrawalRequestStatus? Status { get; init; }
}

// ══════════════════════════════════════════════════════════════════════════════
// USER — My detail
// ══════════════════════════════════════════════════════════════════════════════

public class GetMyWithdrawalRequestDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/withdrawal-requests/me/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyWithdrawalRequestDetailQuery(RequestId: id);
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization()
        .WithName("GetMyWithdrawalRequestDetail")
        .WithTags("Withdrawal Requests")
        .Produces<WithdrawalRequestDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// ADMIN — List
// ══════════════════════════════════════════════════════════════════════════════

public class GetAllWithdrawalRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/withdrawal-requests", async (
            [AsParameters] GetAllWithdrawalRequestsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllWithdrawalRequestsQuery
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder?.ToLower() == "asc"
                                ? SortOrder.Ascending
                                : SortOrder.Descending,
                UserId = request.UserId,
                Status = request.Status,
                CreatedFrom = request.CreatedFrom,
                CreatedTo = request.CreatedTo
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization("Admin")
        .WithName("GetAllWithdrawalRequests")
        .WithTags("Admin - Withdrawal Requests")
        .Produces<PagedResult<WithdrawalRequestAdminListItemDto>>(StatusCodes.Status200OK);
    }
}

public sealed record GetAllWithdrawalRequestsRequestDto
{
    [DefaultValue(1)] public int PageNumber { get; init; } = 1;
    [DefaultValue(10)] public int PageSize { get; init; } = 10;

    [DefaultValue("CreatedAt")] public string SortColumn { get; init; } = "CreatedAt";
    [DefaultValue("desc")] public string SortOrder { get; init; } = "desc";

    public Guid? UserId { get; init; }
    public WithdrawalRequestStatus? Status { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}

// ══════════════════════════════════════════════════════════════════════════════
// ADMIN — Detail
// ══════════════════════════════════════════════════════════════════════════════

public class GetWithdrawalRequestDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/withdrawal-requests/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetWithdrawalRequestDetailQuery(RequestId: id);
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization("Admin")
        .WithName("GetWithdrawalRequestDetail")
        .WithTags("Admin - Withdrawal Requests")
        .Produces<WithdrawalRequestAdminDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// ADMIN — Approve
// ══════════════════════════════════════════════════════════════════════════════

public class ApproveWithdrawalRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/withdrawal-requests/{id:guid}/approve", async (
            Guid id,
            ApproveWithdrawalRequestDto body,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ApproveWithdrawalRequestCommand(
                RequestId: id,
                AdminNote: body.AdminNote);

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization("Admin")
        .WithName("ApproveWithdrawalRequest")
        .WithTags("Admin - Withdrawal Requests")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record ApproveWithdrawalRequestDto(string? AdminNote);

// ══════════════════════════════════════════════════════════════════════════════
// ADMIN — Reject
// ══════════════════════════════════════════════════════════════════════════════

public class RejectWithdrawalRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/withdrawal-requests/{id:guid}/reject", async (
            Guid id,
            RejectWithdrawalRequestDto body,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RejectWithdrawalRequestCommand(
                RequestId: id,
                AdminNote: body.AdminNote);

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization("Admin")
        .WithName("RejectWithdrawalRequest")
        .WithTags("Admin - Withdrawal Requests")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record RejectWithdrawalRequestDto(string AdminNote);  // required

// ══════════════════════════════════════════════════════════════════════════════
// ADMIN — Complete
// ══════════════════════════════════════════════════════════════════════════════

public class CompleteWithdrawalRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/withdrawal-requests/{id:guid}/complete", async (
            Guid id,
            CompleteWithdrawalRequestDto body,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteWithdrawalRequestCommand(
                RequestId: id,
                AdminNote: body.AdminNote);

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization("Admin")
        .WithName("CompleteWithdrawalRequest")
        .WithTags("Admin - Withdrawal Requests")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record CompleteWithdrawalRequestDto(string? AdminNote);

// ══════════════════════════════════════════════════════════════════════════════
// ADMIN — Fail  (bank transfer failed after approval → auto-refunds wallet)
// ══════════════════════════════════════════════════════════════════════════════

public class FailWithdrawalRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/withdrawal-requests/{id:guid}/fail", async (
            Guid id,
            FailWithdrawalRequestDto body,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new FailWithdrawalRequestCommand(
                RequestId: id,
                AdminNote: body.AdminNote);

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        //.RequireAuthorization("Admin")
        .WithName("FailWithdrawalRequest")
        .WithTags("Admin - Withdrawal Requests")
        .WithSummary("Mark an approved withdrawal as failed")
        .WithDescription(
            "Admin calls this when the bank transfer could not be completed. " +
            "Marks the original debit transaction as failed and automatically " +
            "refunds the full amount back to the user's wallet.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record FailWithdrawalRequestDto(string AdminNote);