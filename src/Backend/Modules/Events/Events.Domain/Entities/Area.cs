using Events.Domain.Enums;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class Area : Entity<Guid>
{
    private readonly List<Seat> _seats = [];

    private Area() { }

    public Guid EventId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Capacity { get; private set; }
    public AreaType Type { get; private set; }

    public Event Event { get; private set; } = null!;
    public IReadOnlyCollection<Seat> Seats => _seats.AsReadOnly();

    public static Area Create(
        Guid eventId,
        string name,
        decimal price,
        int capacity,
        AreaType type)
    {
        return new Area
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Name = name,
            Price = price,
            Capacity = capacity,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddSeat(Seat seat) => _seats.Add(seat);

    public void UpdatePrice(decimal price)
    {
        Price = price;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateCapacity(int capacity)
    {
        Capacity = capacity;
        ModifiedAt = DateTime.UtcNow;
    }
}