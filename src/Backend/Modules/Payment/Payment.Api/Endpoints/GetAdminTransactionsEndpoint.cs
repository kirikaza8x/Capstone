using System.ComponentModel;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Domain.Enums;
using Payments.Application.DTOs.Payment;
using Payments.Application.Features.Payments.Queries.GetAdminTransactions;
using Shared.Api.Results;
using Shared.Domain.Pagination;

namespace Payments.Api.Features.Payments.GetAdminTransactions;

public class GetAdminTransactionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments", async (
            [AsParameters] GetAdminTransactionsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAdminTransactionsQuery
            {
                UserId = request.UserId,
                OrderId = request.OrderId,
                EventId = request.EventId,
                Type = request.Type,
                ReferenceType = request.ReferenceType,
                Status = request.Status,
                AmountMin = request.AmountMin,
                AmountMax = request.AmountMax,
                CreatedFrom = request.CreatedFrom,
                CreatedTo = request.CreatedTo,
                GatewayTxnRef = request.GatewayTxnRef,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder?.ToLower() == "asc"
                    ? Shared.Domain.Queries.SortOrder.Ascending
                    : Shared.Domain.Queries.SortOrder.Descending
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetAdminTransactions")
        .WithTags("Payments")
        .Produces<PagedResult<PaymentTransactionDto>>(StatusCodes.Status200OK)
        .WithSummary("Admin: paginated list of all payment transactions with filters");
    }
}

public sealed record GetAdminTransactionsRequestDto
{
    [DefaultValue(1)] public int PageNumber { get; init; } = 1;
    [DefaultValue(10)] public int PageSize { get; init; } = 10;

    [DefaultValue("CreatedAt")] public string SortColumn { get; init; } = "CreatedAt";
    [DefaultValue("desc")] public string SortOrder { get; init; } = "desc";

    public Guid? UserId { get; init; }
    public Guid? OrderId { get; init; }
    public Guid? EventId { get; init; }
    public PaymentType? Type { get; init; }
    public PaymentReferenceType? ReferenceType { get; init; }
    public PaymentInternalStatus? Status { get; init; }
    public decimal? AmountMin { get; init; }
    public decimal? AmountMax { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public string? GatewayTxnRef { get; init; }
}
