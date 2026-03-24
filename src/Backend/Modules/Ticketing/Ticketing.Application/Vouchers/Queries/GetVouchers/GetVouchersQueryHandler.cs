using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Ticketing.Application.Vouchers.Queries.Dto;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Vouchers.Queries.GetVouchers;

internal sealed class GetVouchersQueryHandler(
    IVoucherRepository voucherRepository,
    IMapper mapper) : IQueryHandler<GetVouchersQuery, PagedResult<VoucherDto>>
{
    public async Task<Result<PagedResult<VoucherDto>>> Handle(
        GetVouchersQuery query,
        CancellationToken cancellationToken)
    {
        var pagedVouchers = await voucherRepository.GetPagedAsync(
            query.EventId,
            query,
            cancellationToken);

        var items = mapper.Map<IReadOnlyList<VoucherDto>>(pagedVouchers.Items);

        return Result.Success(PagedResult<VoucherDto>.Create(
            items,
            pagedVouchers.PageNumber,
            pagedVouchers.PageSize,
            pagedVouchers.TotalCount));
    }
}
