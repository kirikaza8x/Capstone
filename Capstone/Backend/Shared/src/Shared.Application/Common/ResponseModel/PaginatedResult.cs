namespace Shared.Application.Common.ResponseModel.Pagination;

public class PaginatedResult<TEntity> where TEntity : class
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public long Count { get; init; }
    public IEnumerable<TEntity> Data { get; init; } = Enumerable.Empty<TEntity>();

    public PaginatedResult(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Count = count;
        Data = data ?? Enumerable.Empty<TEntity>();
    }
}
