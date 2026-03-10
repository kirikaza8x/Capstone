using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventCategoryConfiguration : IEntityTypeConfiguration<EventCategory>
{
    public void Configure(EntityTypeBuilder<EventCategory> builder)
    {
        builder.ToTable("event_categories");

        builder.HasKey(e => new { e.EventId, e.CategoryId });

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}