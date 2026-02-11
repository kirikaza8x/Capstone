using Events.Domain.Entities;
using Events.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("ticket_types");

        builder.HasKey(e => e.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.SoldQuantity).HasDefaultValue(0);

        builder.Property(e => e.Type)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<AreaType>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.AreaId).IsRequired(false);

        builder.HasIndex(e => e.EventSessionId);
        builder.HasIndex(e => e.AreaId);

        builder.HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(e => e.AvailableQuantity);
        builder.Ignore(e => e.IsSoldOut);
        builder.Ignore(e => e.IsFree);
    }
}