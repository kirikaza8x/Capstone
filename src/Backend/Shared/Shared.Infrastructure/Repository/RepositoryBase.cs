using Microsoft.EntityFrameworkCore;
using Shared.Domain.Data;
using Shared.Domain.Queries;
using Shared.Domain.DDD;
using Shared.Domain.Pagination;
using System.Linq.Expressions;
using Shared.Infrastructure.Extensions;

namespace Shared.Infrastructure.Repository;

public abstract class RepositoryBase<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected RepositoryBase(DbContext context)
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

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
            PagedQuery pagedQuery,
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        var sortedQuery = query.ApplySorting(pagedQuery);

        if (string.IsNullOrWhiteSpace(pagedQuery.SortColumn) &&
            typeof(TEntity).GetProperty("CreatedAt") != null)
        {
            sortedQuery = sortedQuery.OrderByDescending(e => EF.Property<DateTime?>(e, "CreatedAt"));
        }

        return await sortedQuery.ToPagedResultAsync(pagedQuery, cancellationToken);
    }

    public virtual async Task<PagedResult<TResult>> GetPagedAsync<TResult>(
        PagedQuery pagedQuery,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortedQuery = query.ApplySorting(pagedQuery);

        var items = await sortedQuery
            .Select(selector)
            .Skip(pagedQuery.Skip)
            .Take(pagedQuery.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<TResult>.Create(
            items,
            pagedQuery.PageNumber,
            pagedQuery.PageSize,
            totalCount);
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

    public virtual void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        DbSet.AddRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }
}
