namespace Events.Domain.Entities;

public sealed class EventHashtag
{
    private EventHashtag() { }

    public Guid EventId { get; private set; }
    public int HashtagId { get; private set; }

    public Event Event { get; private set; } = null!;
    public Hashtag Hashtag { get; private set; } = null!;

    public static EventHashtag Create(Guid eventId, int hashtagId)
    {
        return new EventHashtag
        {
            EventId = eventId,
            HashtagId = hashtagId
        };
    }
}