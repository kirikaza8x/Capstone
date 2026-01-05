using Microsoft.EntityFrameworkCore;
using Shared.Application.Queries;
using Shared.Domain.Pagination;
using System.Linq.Expressions;
using System.Reflection;

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
    /// Convert IQueryable to PagedList with PagedQuery parameters
    /// </summary>
    public static Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery,
        CancellationToken cancellationToken = default)
    {
        return query.ToPagedListAsync(
            pagedQuery.PageNumber,
            pagedQuery.PageSize,
            cancellationToken);
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

    /// <summary>
    /// Apply pagination from PagedQuery
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery)
    {
        return query.ApplyPagination(pagedQuery.PageNumber, pagedQuery.PageSize);
    }

    /// <summary>
    /// Apply sorting dynamically by column name
    /// Supports: "asc", "desc"
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortColumn,
        string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(
            sortColumn,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            return query;
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.PropertyType],
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Apply sorting from PagedQuery
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery)
    {
        return query.ApplySorting(pagedQuery.SortColumn, pagedQuery.SortOrder);
    }

    /// <summary>
    /// Apply all PagedQuery filters (sorting + pagination)
    /// </summary>
    public static IQueryable<T> ApplyPagedQuery<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery)
    {
        return query
            .ApplySorting(pagedQuery)
            .ApplyPagination(pagedQuery);
    }
}
