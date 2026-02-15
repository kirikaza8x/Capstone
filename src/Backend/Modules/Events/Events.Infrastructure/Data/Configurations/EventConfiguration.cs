using Events.Domain.Entities;
using Events.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrganizerId).IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<EventStatus>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.TicketSaleStartAt).IsRequired(false);
        builder.Property(e => e.TicketSaleEndAt).IsRequired(false);
        builder.Property(e => e.EventStartAt).IsRequired(false);
        builder.Property(e => e.EventEndAt).IsRequired(false);

        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.BannerUrl).HasColumnType("text");
        builder.Property(e => e.Location).HasMaxLength(500);
        builder.Property(e => e.MapUrl).HasMaxLength(500);
        builder.Property(e => e.Policy).HasColumnType("text");
        builder.Property(e => e.Spec).HasColumnType("jsonb");
        builder.Property(e => e.SeatmapImage).HasMaxLength(500);
        builder.Property(e => e.UrlPath).HasMaxLength(255).IsRequired();
        builder.Property(e => e.EventTypeId).IsRequired(false);
        builder.Property(e => e.EventCategoryId).IsRequired();
        builder.Property(e => e.IsEmailReminderEnabled).HasDefaultValue(false);

        builder.HasIndex(e => e.UrlPath).IsUnique();
        builder.HasIndex(e => e.OrganizerId);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasMany(e => e.Sessions)
            .WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Images)
            .WithOne(i => i.Event)
            .HasForeignKey(i => i.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.CategoryMappings)
            .WithOne(cm => cm.Event)
            .HasForeignKey(cm => cm.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EventHashtags)
            .WithOne(eh => eh.Event)
            .HasForeignKey(eh => eh.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Staffs)
            .WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ActorImages)
            .WithOne(ai => ai.Event)
            .HasForeignKey(ai => ai.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Areas)
            .WithOne(a => a.Event)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}