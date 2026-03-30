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

        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        // config gin index for title
        builder.HasIndex(e => e.Title)
            .HasDatabaseName("ix_events_title_trigram")
            .HasMethod("gin") 
            .HasOperators("gin_trgm_ops");

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
        builder.Property(e => e.SpecImage).HasColumnType("text");
        builder.Property(e => e.IsEmailReminderEnabled).HasDefaultValue(false);
        builder.Property(e => e.ReminderTriggeredAt).IsRequired(false);
        builder.Property(e => e.CancellationReason).HasColumnType("text");
        builder.Property(e => e.PublishRejectionReason).HasColumnType("text");
        builder.Property(e => e.CancellationRejectionReason).HasColumnType("text");
        builder.Property(e => e.SuspensionReason).HasColumnType("text");
        builder.Property(e => e.SuspendedAt).IsRequired(false);
        builder.Property(e => e.SuspendedUntilAt).IsRequired(false);
        builder.Property(e => e.SuspendedBy).IsRequired(false);

        builder.HasIndex(e => e.UrlPath)
            .IsUnique()
            .HasFilter("\"url_path\" IS NOT NULL");
        builder.HasIndex(e => e.OrganizerId);
        builder.HasIndex(e => e.Status);

        builder.HasMany(e => e.Sessions)
            .WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.TicketTypes)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Images)
            .WithOne(i => i.Event)
            .HasForeignKey(i => i.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EventCategories)
            .WithOne(cm => cm.Event)
            .HasForeignKey(cm => cm.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EventHashtags)
            .WithOne(eh => eh.Event)
            .HasForeignKey(eh => eh.EventId);

        builder.HasMany(e => e.Members)
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
