using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Ticketing.Application.Vouchers.Queries.Dto;

namespace Ticketing.Application.Vouchers.Queries.GetVouchers;

public sealed record GetVouchersQuery : PagedQuery, IQuery<PagedResult<VoucherDto>>
{
    public Guid? EventId { get; init; }
}
