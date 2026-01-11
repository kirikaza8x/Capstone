using Microsoft.EntityFrameworkCore;
using Shared.Domain.Queries;
using Shared.Domain.Pagination;
using System.Linq.Expressions;
using System.Reflection;

namespace Shared.Infrastructure.Extensions;

public static class QueryableExtension
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
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

        return PagedResult<T>.Create(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public static Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery,
        CancellationToken cancellationToken = default)
    {
        return query.ToPagedResultAsync(
            pagedQuery.PageNumber,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery)
    {
        return query.ApplyPagination(pagedQuery.PageNumber, pagedQuery.PageSize);
    }

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

        var methodName = sortOrder?.ToLower() == "desc"
            ? "OrderByDescending"
            : "OrderBy";

        var orderByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, orderByExpression })!;
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery)
    {
        return query.ApplySorting(pagedQuery.SortColumn, pagedQuery.SortOrder);
    }
}
