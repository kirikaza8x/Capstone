using Shared.Domain.DDD;

namespace Shared.Domain.Data;

public partial interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    Task BulkInsertAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    Task BulkUpdateAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    Task BulkDeleteAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    Task BulkMergeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);
}
