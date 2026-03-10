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

    public virtual async Task<IReadOnlyList<TEntity>> SearchAsync(
        Expression<Func<TEntity, string>> field,
        string keyword,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var parameter = field.Parameters[0];
        var lowerField = Expression.Call(field.Body, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);
        var lowerKeyword = Expression.Constant($"%{keyword.ToLowerInvariant()}%");

        var likeMethod = typeof(DbFunctionsExtensions)
            .GetMethod(nameof(DbFunctionsExtensions.Like),
                [typeof(DbFunctions), typeof(string), typeof(string)])!;

        var body = Expression.Call(
            null,
            likeMethod,
            Expression.Constant(EF.Functions),
            lowerField,
            lowerKeyword);

        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        return await DbSet
            .AsNoTracking()
            .Where(predicate)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<Domain.Pagination.PagedResult<TEntity>> GetAllWithPagingAsync(
        PagedQuery pagedQuery,
        Expression<Func<TEntity, bool>>? predicate = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        if (includes is not null)
        {
            foreach (var include in includes)
                query = query.Include(include);

            query = query.AsSplitQuery();
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
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> dbQuery = DbSet.AsNoTracking();

        if (predicate is not null)
            dbQuery = dbQuery.Where(predicate);

        if (includes is not null)
        {
            foreach (var include in includes)
                dbQuery = dbQuery.Include(include);

            dbQuery = dbQuery.AsSplitQuery();
        }

        dbQuery = dbQuery.ApplyDynamicFilters(query.Filter);

        dbQuery = (query.Sorts == null || !query.Sorts.Any())
            ? dbQuery.ApplyDynamicSorting(new List<Sort> { new Sort { Field = "CreatedAt", Dir = "desc" } })
            : dbQuery.ApplyDynamicSorting(query.Sorts);

        return await dbQuery.ToPagedResultAsync(query, cancellationToken);
    }

    public virtual async Task<Shared.Domain.Pagination.PagedResult<TResult>> GetPagedAsync<TResult>(
        AdvancedPagedQuery query,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> dbQuery = DbSet.AsNoTracking();

        if (predicate is not null)
            dbQuery = dbQuery.Where(predicate);

        if (includes is not null)
        {
            foreach (var include in includes)
                dbQuery = dbQuery.Include(include);

            dbQuery = dbQuery.AsSplitQuery();
        }

        dbQuery = dbQuery.ApplyDynamicFilters(query.Filter);

        dbQuery = (query.Sorts == null || !query.Sorts.Any())
            ? dbQuery.ApplyDynamicSorting(new List<Sort> { new Sort { Field = "CreatedAt", Dir = "desc" } })
            : dbQuery.ApplyDynamicSorting(query.Sorts);

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
