using Payments.Application.Features.WithdrawalRequests.Dtos;
using Payments.Domain.Enums;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Payments.Application.Features.WithdrawalRequests.Queries;

public sealed record GetAllWithdrawalRequestsQuery : PagedQuery,
    IQuery<PagedResult<WithdrawalRequestAdminListItemDto>>
{
    public Guid? UserId { get; init; }
    public WithdrawalRequestStatus? Status { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}

public sealed record GetWithdrawalRequestDetailQuery(Guid RequestId)
    : IQuery<WithdrawalRequestAdminDetailDto>;
