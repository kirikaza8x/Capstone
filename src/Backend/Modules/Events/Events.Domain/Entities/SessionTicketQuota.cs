using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class SessionTicketQuota : Entity<Guid>
{
    private SessionTicketQuota() { }

    public Guid EventSessionId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public int Quantity { get; private set; }

    public EventSession EventSession { get; private set; } = null!;
    public TicketType TicketType { get; private set; } = null!;

    public static SessionTicketQuota Create(Guid eventSessionId, Guid ticketTypeId, int quantity)
    {
        return new SessionTicketQuota
        {
            Id = Guid.NewGuid(),
            EventSessionId = eventSessionId,
            TicketTypeId = ticketTypeId,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
        ModifiedAt = DateTime.UtcNow;
    }
}