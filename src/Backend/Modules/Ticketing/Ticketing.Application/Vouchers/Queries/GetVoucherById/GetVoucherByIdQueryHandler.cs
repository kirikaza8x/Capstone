using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Application.Vouchers.Queries.Dto;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Vouchers.Queries.GetVoucherById;

internal sealed class GetVoucherByIdQueryHandler(
    IVoucherRepository voucherRepository,
    IMapper mapper) : IQueryHandler<GetVoucherByIdQuery, VoucherDto>
{
    public async Task<Result<VoucherDto>> Handle(
        GetVoucherByIdQuery query,
        CancellationToken cancellationToken)
    {
        var voucher = await voucherRepository.GetByIdAsync(
            query.Id,
            cancellationToken);

        if (voucher is null)
            return Result.Failure<VoucherDto>(
                TicketingErrors.Voucher.NotFound(query.Id.ToString()));

        return Result.Success(mapper.Map<VoucherDto>(voucher));
    }
}
