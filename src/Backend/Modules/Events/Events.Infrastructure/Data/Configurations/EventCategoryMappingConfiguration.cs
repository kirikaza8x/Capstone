using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventCategoryMappingConfiguration : IEntityTypeConfiguration<EventCategoryMapping>
{
    public void Configure(EntityTypeBuilder<EventCategoryMapping> builder)
    {
        builder.ToTable("event_category_mappings");

        builder.HasKey(e => e.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.HasIndex(e => new { e.EventId, e.CategoryId })
            .IsUnique();

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}