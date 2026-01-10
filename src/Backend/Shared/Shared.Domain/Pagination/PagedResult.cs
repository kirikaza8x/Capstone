namespace Shared.Domain.Pagination;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int CurrentPageSize => Items.Count;
    public int CurrentStartIndex => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
    public int CurrentEndIndex => TotalCount == 0 ? 0 : CurrentStartIndex + CurrentPageSize - 1;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

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
