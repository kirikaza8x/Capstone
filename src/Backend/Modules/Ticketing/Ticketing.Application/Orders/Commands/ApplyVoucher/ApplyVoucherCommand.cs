using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Commands.ApplyVoucher;

public sealed record ApplyVoucherCommand(
    Guid OrderId,
    string CouponCode) : ICommand<ApplyVoucherResponse>;

public sealed record ApplyVoucherResponse(
    Guid OrderId,
    decimal OriginalPrice,
    decimal DiscountAmount,
    decimal TotalPrice);
