using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.Data.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.TotalPrice).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<OrderStatus>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasMany(x => x.Tickets)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OrderVouchers)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
    }
}
