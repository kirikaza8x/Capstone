using Payments.Application.DTOs.Refund;
using Payments.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.Queries.GetPendingRefundRequests;

public record GetPendingRefundRequestsQuery(
    RefundRequestStatus? StatusFilter = null,   
    int Page = 1,
    int PageSize = 20
) : IQuery<GetPendingRefundRequestsResult>;

public record GetPendingRefundRequestsResult(
    IReadOnlyList<RefundRequestDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);