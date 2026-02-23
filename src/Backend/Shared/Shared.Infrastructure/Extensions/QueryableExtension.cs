using Microsoft.EntityFrameworkCore;
using Shared.Domain.Queries;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace Shared.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for IQueryable to support dynamic filtering, sorting, and asynchronous pagination.
/// </summary>
public static class QueryableExtensions
{
    private static readonly IDictionary<string, string> Operators = new Dictionary<string, string>
    {
        {"eq", "="}, {"neq", "!="}, {"lt", "<"}, {"lte", "<="},
        {"gt", ">"}, {"gte", ">="},
        {"startswith", "StartsWith"}, {"endswith", "EndsWith"},
        {"contains", "Contains"}, {"doesnotcontain", "Contains"}
    };

    /// <summary>
    /// Applies complex, nested filtering to the query using System.Linq.Dynamic.Core.
    /// <example>
    /// <code>
    /// var filter = new Filter { Field = "Email", Operator = "contains", Value = "@gmail.com" };
    /// query = query.ApplyDynamicFilters(filter);
    /// </code>
    /// </example>
    /// </summary>
    public static IQueryable<T> ApplyDynamicFilters<T>(this IQueryable<T> query, Filter? filter)
    {
        if (filter == null) return query;

        bool hasChildFilters = filter.Filters != null && filter.Filters.Any();
        bool hasValidCurrentFilter = !string.IsNullOrWhiteSpace(filter.Field) &&
                                     !string.IsNullOrWhiteSpace(filter.Operator) &&
                                     Operators.ContainsKey(filter.Operator.ToLower());

        if (hasChildFilters || hasValidCurrentFilter)
        {
            var filters = GetAllFilters(filter);
            if (!filters.Any()) return query;

            var values = filters.Select(f => ExtractValue(f.Value)).ToArray();
            var where = Transform(filter, filters);

            if (string.IsNullOrWhiteSpace(where) || where == "()") return query;

            return query.Where(where, values);
        }

        return query;
    }

    /// <summary>
    /// Applies multiple sorting criteria. Guards against null collections and empty field names.
    /// <example>
    /// <code>
    /// query = query.ApplyDynamicSorting(new List&lt;Sort&gt; { new Sort { Field = "Id", Dir = "asc" } });
    /// </code>
    /// </example>
    /// </summary>
    public static IQueryable<T> ApplyDynamicSorting<T>(this IQueryable<T> query, IEnumerable<Sort>? sorts)
    {
        if (sorts == null) return query;

        var validSorts = sorts
            .Where(s => !string.IsNullOrWhiteSpace(s.Field))
            .ToList();

        if (!validSorts.Any()) return query;

        var ordering = string.Join(",", validSorts.Select(s => $"{s.Field} {s.Dir}"));
        return !string.IsNullOrWhiteSpace(ordering) ? query.OrderBy(ordering) : query;
    }

    /// <summary>
    /// Flatten recursive filters.
    /// </summary>
    private static IList<Filter> GetAllFilters(Filter filter)
    {
        var filters = new List<Filter>();
        GetFilters(filter, filters);
        return filters;
    }

    /// <summary>
    /// Recursive traversal helper.
    /// </summary>
    private static void GetFilters(Filter filter, IList<Filter> filters)
    {
        if (filter.Filters != null && filter.Filters.Any())
        {
            foreach (var item in filter.Filters) GetFilters(item, filters);
        }
        else if (!string.IsNullOrWhiteSpace(filter.Field) && !string.IsNullOrWhiteSpace(filter.Operator))
        {
            filters.Add(filter);
        }
    }

    /// <summary>
    /// Transforms Filter to LINQ string. 
    /// GUARDED: Uses TryGetValue to prevent KeyNotFoundException on invalid operators.
    /// </summary>
    private static string Transform(Filter filter, IList<Filter> filters)
    {
        if (filter.Filters != null && filter.Filters.Any())
        {
            var children = filter.Filters
                .Select(f => Transform(f, filters))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            return children.Length > 0 ? $"({string.Join($" {filter.Logic} ", children)})" : "";
        }

        int index = filters.IndexOf(filter);
        if (index < 0) return "";

        // GUARD: Handle unknown operators gracefully
        if (string.IsNullOrWhiteSpace(filter.Operator) || !Operators.TryGetValue(filter.Operator.ToLower(), out var comparison))
        {
            return ""; 
        }

        if (filter.Operator.Equals("doesnotcontain", StringComparison.OrdinalIgnoreCase))
        {
            return $"(!{filter.Field}.Contains(@{index}))";
        }

        if (comparison == "StartsWith" || comparison == "EndsWith" || comparison == "Contains")
        {
            return $"({filter.Field}.{comparison}(@{index}))";
        }

        return $"{filter.Field} {comparison} @{index}";
    }

    /// <summary>
    /// Extracts value from JsonElement.
    /// GUARDED: Handles Null/None ValueKinds to prevent NullReferenceException.
    /// </summary>
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
                JsonValueKind.Null => null, 
                JsonValueKind.Undefined => null, 
                _ => element.ToString()
            };
        }
        return value;
    }

    /// <summary>
    /// Executes query and returns Domain-specific PagedResult.
    /// </summary>
    public static async Task<Domain.Pagination.PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Domain.Pagination.PagedResult<T>.Create(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Helper for PagedQuery objects.
    /// </summary>
    public static Task<Domain.Pagination.PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedQuery pagedQuery,
        CancellationToken cancellationToken = default)
    {
        return query.ToPagedResultAsync(pagedQuery.PageNumber, pagedQuery.PageSize, cancellationToken);
    }
}