using System.Linq.Expressions;
using Shared.Domain.DDD;

namespace Shared.Domain.Data.Repositories;

public partial interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    // ... your existing methods ...

    /// <summary>
    /// Upserts an entity based on its primary key (Id).
    /// </summary>
    void Upsert(TEntity entity);

    /// <summary>
    /// Upserts an entity based on a custom predicate. 
    /// Useful for checking unique constraints like Email or Code.
    /// </summary>
    Task UpsertAsync(
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}
