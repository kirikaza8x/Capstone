namespace Shared.Domain.DDD;

public abstract class Entity<T> : IEntity<T>
{
    public T Id { get; set; } = default!;

    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsActive { get; set; } = true;

    protected Entity() { }

    protected Entity(T id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<T> entity)
        {
            return false;
        }

        if (ReferenceEquals(this, entity))
        {
            return true;
        }

        if (GetType() != entity.GetType())
        {
            return false;
        }

        return EqualityComparer<T>.Default.Equals(Id, entity.Id) && !EqualityComparer<T>.Default.Equals(Id, default);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<T>? left, Entity<T>? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Entity<T>? left, Entity<T>? right)
    {
        return !(left == right);
    }

}