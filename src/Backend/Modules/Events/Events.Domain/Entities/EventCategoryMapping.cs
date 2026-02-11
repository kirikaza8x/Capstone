using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventCategoryMapping
{
    private EventCategoryMapping() { }

    public Guid EventId { get; private set; }
    public int CategoryId { get; private set; }

    public Event Event { get; private set; } = null!;
    public EventCategory Category { get; private set; } = null!;

    public static EventCategoryMapping Create(Guid eventId, int categoryId)
    {
        return new EventCategoryMapping
        {
            EventId = eventId,
            CategoryId = categoryId
        };
    }
}