using Payments.Application.DTOs.Refund;
using Payments.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.Queries.GetMyRefundRequests;

public record GetMyRefundRequestsQuery(
    RefundRequestStatus? StatusFilter = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<GetMyRefundRequestsResult>;

public record GetMyRefundRequestsResult(
    IReadOnlyList<RefundRequestDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
