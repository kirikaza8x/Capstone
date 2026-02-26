using Microsoft.EntityFrameworkCore;
using Shared.Domain.Data;
using Shared.Domain.Queries;
using Shared.Domain.DDD;
using System.Linq.Expressions;
using Shared.Infrastructure.Extensions;
using System.Linq.Dynamic.Core;

namespace Shared.Infrastructure.Data;

public partial class RepositoryBase<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public RepositoryBase(DbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<Domain.Pagination.PagedResult<TEntity>> GetAllWithPagingAsync(
        PagedQuery pagedQuery,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (!string.IsNullOrWhiteSpace(pagedQuery.SortColumn))
        {
            var sortDir = pagedQuery.SortOrder == SortOrder.Descending ? "desc" : "asc";
            query = query.OrderBy($"{pagedQuery.SortColumn} {sortDir}");
        }

        return await query.ToPagedResultAsync(pagedQuery, cancellationToken);
    }

    public virtual async Task<Shared.Domain.Pagination.PagedResult<TEntity>> GetPagedAsync(
        AdvancedPagedQuery query,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> dbQuery = DbSet.AsNoTracking();

        if (predicate is not null)
        {
            dbQuery = dbQuery.Where(predicate);
        }

        dbQuery = dbQuery.ApplyDynamicFilters(query.Filter);

        if (query.Sorts == null || !query.Sorts.Any())
        {
            dbQuery = dbQuery.ApplyDynamicSorting(new List<Sort>
            {
                new Sort { Field = "CreatedAt", Dir = "desc" }
            });
        }
        else
        {
            dbQuery = dbQuery.ApplyDynamicSorting(query.Sorts);
        }

        return await dbQuery.ToPagedResultAsync(query, cancellationToken);
    }

    public virtual async Task<Shared.Domain.Pagination.PagedResult<TResult>> GetPagedAsync<TResult>(
        AdvancedPagedQuery query,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> dbQuery = DbSet.AsNoTracking();

        if (predicate is not null)
        {
            dbQuery = dbQuery.Where(predicate);
        }

        dbQuery = dbQuery.ApplyDynamicFilters(query.Filter);

        if (query.Sorts == null || !query.Sorts.Any())
        {
            dbQuery = dbQuery.ApplyDynamicSorting(new List<Sort>
            {
                new Sort { Field = "CreatedAt", Dir = "desc" }
            });
        }
        else
        {
            dbQuery = dbQuery.ApplyDynamicSorting(query.Sorts);
        }

        return await dbQuery
            .Select(selector)
            .ToPagedResultAsync(query, cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }
}
