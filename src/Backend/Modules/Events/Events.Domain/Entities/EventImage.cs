using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventImage : Entity<Guid>
{
    private EventImage() { }

    public Guid EventId { get; private set; }
    public string? ImageUrl { get; private set; }

    public Event Event { get; private set; } = null!;

    public static EventImage Create(Guid eventId, string? imageUrl)
    {
        return new EventImage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };
    }
}