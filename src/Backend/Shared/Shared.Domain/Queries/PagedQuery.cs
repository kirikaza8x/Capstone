namespace Shared.Domain.Queries;

public abstract record PagedQuery : IPageable, ISortable
{
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
    public string? SortColumn { get; init; } = "CreatedAt";
    public SortOrder? SortOrder { get; init; } = Queries.SortOrder.Descending;
}
