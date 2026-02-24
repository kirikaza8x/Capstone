namespace Shared.Domain.Queries;

public interface IPageable
{
    int? PageNumber { get; }
    int? PageSize { get; }
}