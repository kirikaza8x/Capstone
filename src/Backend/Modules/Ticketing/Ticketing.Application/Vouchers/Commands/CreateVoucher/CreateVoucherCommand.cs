using Shared.Application.Abstractions.Messaging;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Vouchers.Commands.CreateVoucher;

public sealed record CreateVoucherCommand(
    string Name,
    string? Description,
    string CouponCode,
    VoucherType Type,
    decimal Value,
    int MaxUse,
    DateTime StartDate,
    DateTime EndDate,
    Guid? EventId) : ICommand<Guid>;
