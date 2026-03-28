using Shared.Domain.Abstractions;
using Shared.Domain.DDD;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;

namespace Ticketing.Domain.Entities;

public sealed class Voucher : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string CouponCode { get; private set; } = string.Empty;
    public VoucherType Type { get; private set; }
    public decimal Value { get; private set; }
    public int TotalUse { get; private set; }
    public int MaxUse { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public Guid? EventId { get; private set; }

    private Voucher() { }

    public static Result<Voucher> Create(
        string name,
        string couponCode,
        VoucherType type,
        decimal value,
        int maxUse,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        Guid? eventId = null,
        DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Voucher>(TicketingErrors.Voucher.InvalidName); 

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
            Name = name.Trim(),
            Description = description?.Trim(),
            CouponCode = couponCode.Trim().ToUpperInvariant(),
            Type = type,
            Value = value,
            TotalUse = 0,
            MaxUse = maxUse,
            StartDate = startDate,
            EndDate = endDate,
            EventId = eventId,
            CreatedAt = utcNow ?? DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public Result Update(
        string name,
        string couponCode,
        VoucherType type,
        decimal value,
        int maxUse,
        DateTime startDate,
        DateTime endDate,
        string? description = null)
    {
        if (TotalUse > 0)
            return Result.Failure(TicketingErrors.Voucher.CannotUpdateUsedVoucher);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TicketingErrors.Voucher.InvalidName);

        if (string.IsNullOrWhiteSpace(couponCode))
            return Result.Failure(TicketingErrors.Voucher.InvalidCouponCode);

        if (value <= 0)
            return Result.Failure(TicketingErrors.Voucher.InvalidValue);

        if (maxUse <= 0)
            return Result.Failure(TicketingErrors.Voucher.InvalidMaxUse);

        if (startDate >= endDate)
            return Result.Failure(TicketingErrors.Voucher.InvalidDateRange);

        Name = name.Trim();
        Description = description?.Trim();
        CouponCode = couponCode.Trim().ToUpperInvariant();
        Type = type;
        Value = value;
        MaxUse = maxUse;
        StartDate = startDate;
        EndDate = endDate;

        return Result.Success();
    }

    public Result CanDelete()
    {
        if (TotalUse > 0)
            return Result.Failure(TicketingErrors.Voucher.CannotDeleteUsedVoucher);

        return Result.Success();
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
