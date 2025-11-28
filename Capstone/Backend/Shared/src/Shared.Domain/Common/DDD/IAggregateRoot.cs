namespace Shared.Domain.Common.DDD;

public interface IAggregateRoot<T> : IAggregateRoot, IEntity<T>
{
}

public interface IAggregateRoot : IEntity
{
    /// <summary>
    /// The list of domain events raised by this aggregate.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears and returns all domain events after they've been dispatched.
    /// </summary>
    IDomainEvent[] ClearDomainEvents();

    /// <summary>
    /// Replay a historical event to rebuild aggregate state (event sourcing).
    /// </summary>
    void ReplayEvent(IDomainEvent @event);
}
