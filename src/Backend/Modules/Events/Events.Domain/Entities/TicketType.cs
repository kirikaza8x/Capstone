using Events.Domain.Enums;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class TicketType : Entity<Guid>
{
    private TicketType() { }

    public Guid EventSessionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public int SoldQuantity { get; private set; }
    public AreaType Type { get; private set; }
    public Guid? AreaId { get; private set; }

    public EventSession EventSession { get; private set; } = null!;
    public Area? Area { get; private set; }

    public int AvailableQuantity => Quantity - SoldQuantity;
    public bool IsSoldOut => AvailableQuantity <= 0;
    public bool IsFree => Price == 0;

    public static TicketType Create(
        Guid eventSessionId,
        string name,
        decimal price,
        int quantity,
        AreaType type,
        Guid? areaId = null)
    {
        return new TicketType
        {
            Id = Guid.NewGuid(),
            EventSessionId = eventSessionId,
            Name = name,
            Price = price,
            Quantity = quantity,
            SoldQuantity = 0,
            Type = type,
            AreaId = areaId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, decimal price, int quantity)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
        ModifiedAt = DateTime.UtcNow;
    }

    public void IncreaseSoldQuantity(int count = 1)
    {
        if (SoldQuantity + count > Quantity)
            throw new InvalidOperationException("Not enough tickets available.");
        SoldQuantity += count;
        ModifiedAt = DateTime.UtcNow;
    }
}