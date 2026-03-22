using Events.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class TicketType : Entity<Guid>
{
    private TicketType() { }

    public Guid EventId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public int SoldQuantity { get; private set; }
    public Guid? AreaId { get; private set; }

    public Event Event { get; private set; } = null!;
    public Area? Area { get; private set; }

    public static TicketType Create(Guid eventId, string name, int quantity, decimal price)
    {
        return new TicketType
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Name = name,
            Quantity = quantity,
            SoldQuantity = 0,
            Price = price,
            AreaId = null,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, int quantity, decimal price)
    {
        Name = name;
        Quantity = quantity;
        Price = price;
        ModifiedAt = DateTime.UtcNow;
    }

    public void AssignArea(Guid areaId)
    {
        AreaId = areaId;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UnassignArea()
    {
        AreaId = null;
        ModifiedAt = DateTime.UtcNow;
    }

    public void IncreaseSoldQuantity(int quantity)
    {
        if (quantity <= 0) return;
        SoldQuantity = Math.Min(SoldQuantity + quantity, Quantity);
    }

    public Result DecreaseSoldQuantity(int amount = 1)
    {
        if (amount <= 0)
            return Result.Failure(EventErrors.TicketTypeErrors.InvalidSoldQuantityAmount);

        if (SoldQuantity - amount < 0)
            return Result.Failure(EventErrors.TicketTypeErrors.CannotDecreaseSoldBelowZero);

        SoldQuantity -= amount;
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
