namespace Shared.Domain.Common.DDD;

public abstract class Entity<T> : IEntity<T>
{
    public T Id { get; set; } = default!;

    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsActive { get; set; } = true;

    // EF Core needs a parameterless constructor
    protected Entity() { }

    // /// <summary>
    // /// Set audit fields for creation.
    // /// </summary>
    // public void SetCreated(string userId)
    // {
    //     CreatedBy = userId;
    //     CreatedAt = DateTime.UtcNow;
    //     IsActive = true;
    // }

    // /// <summary>
    // /// Set audit fields for modification.
    // /// </summary>
    // public void SetModified(string userId)
    // {
    //     ModifiedBy = userId;
    //     ModifiedAt = DateTime.UtcNow;
    // }

    // /// <summary>
    // /// Soft delete (deactivate).
    // /// </summary>
    // public void Deactivate(string userId)
    // {
    //     IsActive = false;
    //     SetModified(userId);
    // }

}
