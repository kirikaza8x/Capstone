using Payments.Application.DTOs.Refund;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Refunds.Queries.GetPendingRefundRequests;

public class GetPendingRefundRequestsQueryHandler(
    IRefundRequestRepository refundRequestRepository)
    : IQueryHandler<GetPendingRefundRequestsQuery, GetPendingRefundRequestsResult>
{
    public async Task<Result<GetPendingRefundRequestsResult>> Handle(
        GetPendingRefundRequestsQuery query, CancellationToken cancellationToken)
    {
        var (requests, totalCount) = await refundRequestRepository
            .GetPagedAsync(
                query.StatusFilter,
                query.Page,
                query.PageSize,
                cancellationToken);

        var dtos = requests
            .Select(r => MapToDto(r))
            .ToList();

        return Result.Success(new GetPendingRefundRequestsResult(
            dtos, totalCount, query.Page, query.PageSize));
    }

    private static RefundRequestDto MapToDto(RefundRequest r) => new(
        Id: r.Id,
        UserId: r.UserId,
        PaymentTransactionId: r.PaymentTransactionId,
        EventSessionId: r.EventSessionId,
        Scope: r.Scope,
        Status: r.Status,
        RequestedAmount: r.RequestedAmount,
        UserReason: r.UserReason,
        ReviewerNote: r.ReviewerNote,
        ReviewedByAdminId: r.ReviewedByAdminId,
        ReviewedAt: r.ReviewedAt,
        CreatedAt: r.CreatedAt);
}
