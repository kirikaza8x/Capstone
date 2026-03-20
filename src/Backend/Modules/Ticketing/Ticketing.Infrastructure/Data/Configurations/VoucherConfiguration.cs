using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.Data.Configurations;

internal sealed class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("vouchers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CouponCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<VoucherType>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Condition).HasColumnType("text").IsRequired();
        builder.Property(x => x.Value).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.TotalUse).IsRequired();
        builder.Property(x => x.MaxUsePerUser).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();

        builder.HasIndex(x => x.CouponCode).IsUnique();
    }
}
