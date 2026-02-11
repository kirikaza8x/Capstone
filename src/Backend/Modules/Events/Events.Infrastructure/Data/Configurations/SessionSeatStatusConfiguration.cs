using Events.Domain.Entities;
using Events.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class SessionSeatStatusConfiguration : IEntityTypeConfiguration<SessionSeatStatus>
{
    public void Configure(EntityTypeBuilder<SessionSeatStatus> builder)
    {
        builder.ToTable("session_seat_statuses");

        builder.HasKey(e => e.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<SessionSeatStatusType>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.UserId).IsRequired(false);

        builder.HasIndex(e => e.EventSessionId);
        builder.HasIndex(e => e.SeatId);
        builder.HasIndex(e => new { e.EventSessionId, e.SeatId }).IsUnique();

        builder.HasOne(e => e.Seat)
            .WithMany()
            .HasForeignKey(e => e.SeatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}