using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventSession : Entity<Guid>
{
    private readonly List<TicketType> _ticketTypes = [];

    private EventSession() { }

    public Guid EventId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public Event Event { get; private set; } = null!;
    public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

    public static EventSession Create(
        Guid eventId,
        string title,
        string? description,
        DateTime startTime,
        DateTime endTime)
    {
        return new EventSession
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Title = title,
            Description = description,
            StartTime = startTime,
            EndTime = endTime,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description, DateTime startTime, DateTime endTime)
    {
        Title = title;
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        ModifiedAt = DateTime.UtcNow;
    }

    public void AddTicketType(TicketType ticketType) => _ticketTypes.Add(ticketType);
}