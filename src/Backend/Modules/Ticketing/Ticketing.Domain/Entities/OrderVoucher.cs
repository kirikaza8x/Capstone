using Shared.Domain.DDD;

namespace Ticketing.Domain.Entities;

public sealed class OrderVoucher : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid VoucherId { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DateTime AppliedAt { get; private set; }

    public Order Order { get; private set; } = null!;
    public Voucher Voucher { get; private set; } = null!;

    private OrderVoucher() { }

    public static OrderVoucher Create(
        Guid orderId,
        Guid voucherId,
        decimal discountAmount,
        DateTime appliedAtUtc)
    {
        return new OrderVoucher
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            VoucherId = voucherId,
            DiscountAmount = discountAmount,
            AppliedAt = appliedAtUtc,
            CreatedAt = appliedAtUtc
        };
    }
}
