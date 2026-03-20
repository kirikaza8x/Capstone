using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Data.Configurations;

internal sealed class OrderVoucherConfiguration : IEntityTypeConfiguration<OrderVoucher>
{
    public void Configure(EntityTypeBuilder<OrderVoucher> builder)
    {
        builder.ToTable("order_vouchers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.VoucherId).IsRequired();
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.AppliedAt).IsRequired();

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderVouchers)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Voucher)
            .WithMany()
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrderId, x.VoucherId }).IsUnique();
    }
}
