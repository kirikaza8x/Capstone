using Shared.Domain.Abstractions;
using Shared.Domain.DDD;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;

namespace Ticketing.Domain.Entities;

public sealed class Voucher : Entity<Guid>
{
    public string CouponCode { get; private set; } = string.Empty;
    public VoucherType Type { get; private set; }
    public string Condition { get; private set; } = string.Empty;
    public decimal Value { get; private set; }
    public int TotalUse { get; private set; }
    public short MaxUsePerUser { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private Voucher() { }

    public static Result<Voucher> Create(
        string couponCode,
        VoucherType type,
        string condition,
        decimal value,
        int totalUse,
        short maxUsePerUser,
        DateTime startDate,
        DateTime endDate,
        DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidValue);

        if (value <= 0 || totalUse < 0 || maxUsePerUser < 0)
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidValue);

        if (startDate >= endDate)
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidDateRange);

        var entity = new Voucher
        {
            Id = Guid.NewGuid(),
            CouponCode = couponCode.Trim(),
            Type = type,
            Condition = condition,
            Value = value,
            TotalUse = totalUse,
            MaxUsePerUser = maxUsePerUser,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = utcNow ?? DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public Result EnsureActive(DateTime utcNow)
    {
        if (utcNow < StartDate || utcNow > EndDate)
            return Result.Failure(TicketingErrors.Voucher.NotActive);

        return Result.Success();
    }
}
