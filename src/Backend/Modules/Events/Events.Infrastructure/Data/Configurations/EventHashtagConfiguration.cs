using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class EventHashtagConfiguration : IEntityTypeConfiguration<EventHashtag>
{
    public void Configure(EntityTypeBuilder<EventHashtag> builder)
    {
        builder.ToTable("event_hashtags");

        builder.HasKey(e => new { e.EventId, e.HashtagId });

        builder.HasOne(e => e.Event)
            .WithMany(ev => ev.EventHashtags)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Hashtag)
            .WithMany(h => h.EventHashtags)
            .HasForeignKey(e => e.HashtagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
