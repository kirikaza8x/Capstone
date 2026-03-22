using Payments.Application.DTOs.Refund;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Refunds.Queries.GetMyRefundRequests;

public class GetMyRefundRequestsQueryHandler(
    ICurrentUserService currentUser,
    IRefundRequestRepository refundRequestRepository)
    : IQueryHandler<GetMyRefundRequestsQuery, GetMyRefundRequestsResult>
{
    public async Task<Result<GetMyRefundRequestsResult>> Handle(
        GetMyRefundRequestsQuery query, CancellationToken cancellationToken)
    {
        var (requests, totalCount) = await refundRequestRepository
            .GetPagedByUserIdAsync(
                currentUser.UserId, query.StatusFilter, query.Page, query.PageSize, cancellationToken);

        var dtos = requests
            .Select(r => MapToDto(r))
            .ToList();

        return Result.Success(new GetMyRefundRequestsResult(
            dtos, totalCount, query.Page, query.PageSize));
    }

    private static RefundRequestDto MapToDto(RefundRequest r) => new(
        Id: r.Id,
        UserId: r.UserId,
        PaymentTransactionId: r.PaymentTransactionId,
        EventId: r.EventId,
        Scope: r.Scope,
        Status: r.Status,
        RequestedAmount: r.RequestedAmount,
        UserReason: r.UserReason,
        ReviewerNote: r.ReviewerNote,
        ReviewedByAdminId: r.ReviewedByAdminId,
        ReviewedAt: r.ReviewedAt,
        CreatedAt: r.CreatedAt);
}