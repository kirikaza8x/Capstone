using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.Data.Configurations;

internal sealed class OrderTicketConfiguration : IEntityTypeConfiguration<OrderTicket>
{
    public void Configure(EntityTypeBuilder<OrderTicket> builder)
    {
        builder.ToTable("order_tickets");

        builder.HasKey(x => x.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.EventSessionId).IsRequired();
        builder.Property(x => x.TicketTypeId).IsRequired();
        builder.Property(x => x.SeatId).IsRequired(false);

        builder.Property(x => x.QrCode)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<OrderTicketStatus>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CheckedInAt).IsRequired(false);
        builder.Property(x => x.CheckedInBy).IsRequired(false);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.EventSessionId);
        builder.HasIndex(x => x.TicketTypeId);
        builder.HasIndex(x => x.SeatId);
        builder.HasIndex(x => x.QrCode).IsUnique();

        // Composite index for querying tickets by session, ticket type, and status
        builder.HasIndex(t => new { t.EventSessionId, t.TicketTypeId, t.Status })
               .HasDatabaseName("IX_OrderTicket_Session_TicketType_Status");
    }
}
