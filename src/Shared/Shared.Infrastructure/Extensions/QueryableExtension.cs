using Microsoft.EntityFrameworkCore;
using Shared.Domain.Pagination;

namespace Shared.Infrastructure.Extensions;

public static class QueryableExtension
{
    /// <summary>
    /// Convert IQueryable to PagedList with async execution
    /// Executes Count() and ToList() on database
    /// </summary>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedList<T>.Create(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    /// <summary>
    /// Apply pagination without executing query
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

}
