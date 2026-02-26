using EFCore.BulkExtensions;
using Shared.Domain.Data;

namespace Shared.Infrastructure.Data;

public partial class RepositoryBase<TEntity, TId> : IRepository<TEntity, TId>
{
    public virtual async Task BulkInsertAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await Context.BulkInsertAsync(entities.ToList(), cancellationToken: cancellationToken);
    }

    public virtual async Task BulkUpdateAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await Context.BulkUpdateAsync(entities.ToList(), cancellationToken: cancellationToken);
    }

    public virtual async Task BulkDeleteAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await Context.BulkDeleteAsync(entities.ToList(), cancellationToken: cancellationToken);
    }

    public virtual async Task BulkMergeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await Context.BulkInsertOrUpdateAsync(entities.ToList(), cancellationToken: cancellationToken);
    }
}
