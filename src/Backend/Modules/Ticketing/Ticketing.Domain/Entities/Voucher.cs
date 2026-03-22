using Shared.Domain.Abstractions;
using Shared.Domain.DDD;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;

namespace Ticketing.Domain.Entities;

public sealed class Voucher : Entity<Guid>
{
    public string CouponCode { get; private set; } = string.Empty;
    public VoucherType Type { get; private set; }
    public decimal Value { get; private set; }
    public int TotalUse { get; private set; }
    public int MaxUse { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private Voucher() { }

    public static Result<Voucher> Create(
        string couponCode,
        VoucherType type,
        decimal value,
        int maxUse,
        DateTime startDate,
        DateTime endDate,
        DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidCouponCode);

        if (value <= 0)
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidValue);

        if (maxUse <= 0)
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidMaxUse);

        if (startDate >= endDate)
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidDateRange);

        var entity = new Voucher
        {
            Id = Guid.NewGuid(),
            CouponCode = couponCode.Trim(),
            Type = type,
            Value = value,
            TotalUse = 0,    
            MaxUse = maxUse,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = utcNow ?? DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public void IncrementUsage()
    {
        TotalUse++;
    }

    public void DecrementUsage()
    {
        if (TotalUse > 0)
            TotalUse--;
    }
}
