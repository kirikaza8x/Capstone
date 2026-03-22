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
                query.StatusFilter, query.Page, query.PageSize, cancellationToken);

        var dtos = requests.Select(MapToDto).ToList();

        return Result.Success(new GetPendingRefundRequestsResult(
            dtos, totalCount, query.Page, query.PageSize));
    }

    private static RefundRequestDto MapToDto(RefundRequest r) => new(
        r.Id, r.UserId, r.PaymentTransactionId, r.EventId,
        r.Scope, r.Status, r.RequestedAmount, r.UserReason,
        r.ReviewerNote, r.ReviewedByAdminId, r.ReviewedAt, r.CreatedAt);
}