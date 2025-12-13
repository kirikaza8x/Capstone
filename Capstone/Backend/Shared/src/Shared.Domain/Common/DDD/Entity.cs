namespace Shared.Domain.Common.DDD
{
    public abstract class Entity<T> : IEntity<T>
    {
        public T Id { get; set; } = default!;
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        protected Entity() { }

        /// <summary>
        /// Set audit fields for creation.
        /// </summary>
        public virtual void SetCreated(string userId)
        {
            CreatedBy = userId;
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        /// <summary>
        /// Set audit fields for modification.
        /// </summary>
        public virtual void SetModified(string userId)
        {
            ModifiedBy = userId;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft delete (deactivate).
        /// </summary>
        public virtual void Delete(string userId)
        {
            IsDeleted = true;
            SetModified(userId);
        }

        /// <summary>
        /// Update hook — override in derived entities to apply domain‑specific changes.
        /// </summary>
        //public abstract void Update(object updateRequest, string userId);
    }
}
