using Payments.Application.Features.WithdrawalRequests.Dtos;
using Payments.Domain.Enums;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Payments.Application.Features.WithdrawalRequests.Queries;

public sealed record GetMyWithdrawalRequestsQuery : PagedQuery,
    IQuery<PagedResult<WithdrawalRequestListItemDto>>
{
    public WithdrawalRequestStatus? Status { get; init; }
}

public sealed record GetMyWithdrawalRequestDetailQuery(Guid RequestId)
    : IQuery<WithdrawalRequestDetailDto>;
