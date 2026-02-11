using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventSessionConfiguration : IEntityTypeConfiguration<EventSession>
{
    public void Configure(EntityTypeBuilder<EventSession> builder)
    {
        builder.ToTable("event_sessions");

        builder.HasKey(e => e.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.StartTime).IsRequired();
        builder.Property(e => e.EndTime).IsRequired();

        builder.HasIndex(e => e.EventId);

        builder.HasMany(e => e.TicketTypes)
            .WithOne(t => t.EventSession)
            .HasForeignKey(t => t.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.SessionSeatStatuses)
            .WithOne(s => s.EventSession)
            .HasForeignKey(s => s.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}