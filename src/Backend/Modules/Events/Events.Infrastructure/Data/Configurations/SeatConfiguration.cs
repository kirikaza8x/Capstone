using Events.Domain.Entities;
using Events.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");

        builder.HasKey(e => e.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.SeatCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowLabel)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.ColumnLabel)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.X).IsRequired();
        builder.Property(e => e.Y).IsRequired();

        builder.HasIndex(e => e.AreaId);
        builder.HasIndex(e => new { e.AreaId, e.SeatCode }).IsUnique();
    }
}