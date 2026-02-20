
namespace Shared.Domain.Queries;

public abstract record PagedQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public IEnumerable<Sort>? Sorts { get; init; }
    public Filter? Filter { get; init; }
    public int Skip => (PageNumber - 1) * PageSize;
}
