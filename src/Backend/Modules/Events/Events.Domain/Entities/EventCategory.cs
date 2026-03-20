namespace Events.Domain.Entities;

public sealed class EventCategory
{
    private EventCategory() { }

    public Guid EventId { get; private set; }
    public int CategoryId { get; private set; }

    public Event Event { get; private set; } = null!;
    public Category Category { get; private set; } = null!;

    public static EventCategory Create(Guid eventId, int categoryId)
    {
        return new EventCategory
        {
            EventId = eventId,
            CategoryId = categoryId
        };
    }
}
