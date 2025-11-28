namespace Shared.Domain.Common.DDD;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raise a new domain event: apply it to mutate state and record it for publishing.
    /// </summary>
    protected void RaiseEvent(IDomainEvent @event)
    {
        Apply(@event);              // mutate state
        _domainEvents.Add(@event);  // record event
    }

    /// <summary>
    /// Replay an event from history (used in event sourcing).
    /// Only applies state, does not record the event again.
    /// </summary>
    public void ReplayEvent(IDomainEvent @event)
    {
        Apply(@event);
    }

    /// <summary>
    /// Clears and returns all domain events after they've been dispatched.
    /// </summary>
    public IDomainEvent[] ClearDomainEvents()
    {
        var dequeued = _domainEvents.ToArray();
        _domainEvents.Clear();
        return dequeued;
    }

    /// <summary>
    /// Each aggregate must implement how it applies events to mutate state.
    /// </summary>
    protected abstract void Apply(IDomainEvent @event);
}
