using System.Text.Json.Serialization;

namespace Shared.Domain.Pagination;

public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int CurrentPageSize => Items.Count;

    public int CurrentStartIndex => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

    public int CurrentEndIndex => TotalCount == 0 ? 0 : CurrentStartIndex + CurrentPageSize - 1;

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;

    public PagedResult()
    {
    }

    [JsonConstructor]
    public PagedResult(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResult<T> Empty => new(
        Array.Empty<T>(),
        0,
        0,
        0);

    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
    }
}