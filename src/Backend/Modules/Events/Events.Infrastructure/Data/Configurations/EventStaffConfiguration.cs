using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventStaffConfiguration : IEntityTypeConfiguration<EventStaff>
{
    public void Configure(EntityTypeBuilder<EventStaff> builder)
    {
        builder.ToTable("event_staffs");

        builder.HasKey(e => e.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Permissions)
            .HasColumnType("text[]");

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.AssignedBy)
            .IsRequired();

        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.EventId, e.UserId })
            .IsUnique();
    }
}