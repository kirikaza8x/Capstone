using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventActorImageConfiguration : IEntityTypeConfiguration<EventActorImage>
{
    public void Configure(EntityTypeBuilder<EventActorImage> builder)
    {
        builder.ToTable("event_actor_images");

        builder.HasKey(e => e.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Major)
            .HasMaxLength(255);

        builder.Property(e => e.Image)
            .HasMaxLength(500);

        builder.HasIndex(e => e.EventId);
    }
}
