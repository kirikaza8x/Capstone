namespace Shared.Domain.Queries;

public interface ISortable
{
    string? SortColumn { get; }
    SortOrder? SortOrder { get; }
}