using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventActorImage : Entity<Guid>
{
    private EventActorImage() { }

    public Guid EventId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Major { get; private set; }
    public string? Image { get; private set; }

    public Event Event { get; private set; } = null!;

    public static EventActorImage Create(Guid eventId, string name, string? major, string? image)
    {
        return new EventActorImage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Name = name,
            Major = major,
            Image = image,
            CreatedAt = DateTime.UtcNow
        };
    }
}
