using Microsoft.EntityFrameworkCore;
using Shared.Domain.Queries;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace Shared.Infrastructure.Extensions;

public static class QueryableExtensions
{
    #region Pagination
    public static IQueryable<TEntity> ApplyPagination<TEntity>(
        this IQueryable<TEntity> query, IPageable pageable)
    {
        int page = pageable.PageNumber ?? 1;
        int size = pageable.PageSize ?? 10;
    
        return query.Skip((page - 1) * size).Take(size);
    }

    public static async Task<Domain.Pagination.PagedResult<T>> ToPagedResultAsync<T>(
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

        return Domain.Pagination.PagedResult<T>.Create(items, pageNumber, pageSize, totalCount);
    }

    public static Task<Domain.Pagination.PagedResult<T>> ToPagedResultAsync<T>(
    this IQueryable<T> query,
    PagedQuery pagedQuery,
    CancellationToken cancellationToken = default)
    {
        return query.ToPagedResultAsync(
            pagedQuery.PageNumber ?? 1,
            pagedQuery.PageSize ?? 10,
            cancellationToken);
    }

    public static async Task<Domain.Pagination.PagedResult<T>> ToPagedResultAsync<T>(
    this IQueryable<T> query,
    IPageable pageable,
    CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(pageable).ToListAsync(cancellationToken);

        return Domain.Pagination.PagedResult<T>.Create(
            items,
            pageable.PageNumber ?? 1,
            pageable.PageSize ?? 10,
            totalCount);
    }
    #endregion

    #region Dynamic Sorting
    public static IQueryable<T> ApplyDynamicSorting<T>(this IQueryable<T> query, IEnumerable<Sort>? sorts)
    {
        if (sorts != null && sorts.Any())
        {
            var ordering = string.Join(",", sorts.Select(s => $"{s.Field} {s.Dir}"));
            return query.OrderBy(ordering);
        }
        return query;
    }
    #endregion

    #region Dynamic Filtering
    private static readonly IDictionary<string, string> Operators = new Dictionary<string, string>
    {
        {"eq", "="}, {"neq", "!="}, {"lt", "<"}, {"lte", "<="},
        {"gt", ">"}, {"gte", ">="},
        {"startswith", "StartsWith"}, {"endswith", "EndsWith"},
        {"contains", "Contains"}, {"doesnotcontain", "Contains"}
    };

    public static IQueryable<T> ApplyDynamicFilters<T>(this IQueryable<T> query, Filter? filter)
    {
        if (filter != null && (!string.IsNullOrEmpty(filter.Logic) || !string.IsNullOrEmpty(filter.Field)))
        {
            var filters = GetAllFilters(filter);
            var values = filters.Select(f => ExtractValue(f.Value)).ToArray();
            var where = Transform(filter, filters);
            query = query.Where(where, values);
        }
        return query;
    }

    private static IList<Filter> GetAllFilters(Filter filter)
    {
        var filters = new List<Filter>();
        GetFilters(filter, filters);
        return filters;
    }

    private static void GetFilters(Filter filter, IList<Filter> filters)
    {
        if (filter.Filters != null && filter.Filters.Any())
        {
            foreach (var item in filter.Filters) GetFilters(item, filters);
        }
        else
        {
            filters.Add(filter);
        }
    }

    private static string Transform(Filter filter, IList<Filter> filters)
    {
        if (filter.Filters != null && filter.Filters.Any())
        {
            return "(" + string.Join(" " + filter.Logic + " ",
                filter.Filters.Select(f => Transform(f, filters)).ToArray()) + ")";
        }

        int index = filters.IndexOf(filter);
        var comparison = Operators[filter.Operator!.ToLower()];

        if (filter.Operator.ToLower() == "doesnotcontain")
            return $"(!{filter.Field}.Contains(@{index}))";

        if (comparison == "StartsWith" || comparison == "EndsWith" || comparison == "Contains")
            return $"({filter.Field}.{comparison}(@{index}))";

        return $"{filter.Field} {comparison} @{index}";
    }

    private static object? ExtractValue(object? value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
                JsonValueKind.String => element.TryGetDateTime(out var d) ? d :
                                        element.TryGetGuid(out var g) ? g : element.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => element.ToString()
            };
        }
        return value;
    }
    #endregion
}