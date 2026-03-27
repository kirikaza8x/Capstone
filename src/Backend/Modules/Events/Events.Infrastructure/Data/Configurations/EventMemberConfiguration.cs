using Events.Domain.Entities;
using Events.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventMemberConfiguration : IEntityTypeConfiguration<EventMember>
{
    public void Configure(EntityTypeBuilder<EventMember> builder)
    {
        builder.ToTable("event_members");

        builder.HasKey(e => e.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Permissions)
            .HasColumnType("text[]");

        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<EventMemberStatus>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.AssignedBy)
            .IsRequired();

        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.EventId, e.UserId })
            .IsUnique();
    }
}
