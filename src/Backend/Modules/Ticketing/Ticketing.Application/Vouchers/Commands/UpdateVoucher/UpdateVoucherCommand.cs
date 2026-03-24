using Shared.Application.Abstractions.Messaging;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Vouchers.Commands.UpdateVoucher;

public sealed record UpdateVoucherCommand(
    Guid VoucherId,
    string CouponCode,
    VoucherType Type, 
    decimal Value,
    int MaxUse,
    DateTime StartDate,
    DateTime EndDate) : ICommand;
