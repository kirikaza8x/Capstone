using Shared.Application.Abstractions.Messaging;
using Ticketing.Application.Vouchers.Queries.Dto;

namespace Ticketing.Application.Vouchers.Queries.GetVoucherById;

public sealed record GetVoucherByIdQuery(Guid Id) : IQuery<VoucherDto>;
