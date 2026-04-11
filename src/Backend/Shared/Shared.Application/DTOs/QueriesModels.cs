using System.ComponentModel;

namespace Shared.Application.Dtos.Queries;

public record PagedRequestDto
{
    [DefaultValue(1)]
    public int PageNumber { get; init; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;

    [DefaultValue("")]
    public string? SearchTerm { get; init; }

    public IEnumerable<SortRequestDto>? Sorts { get; init; } = new List<SortRequestDto>();

    [DefaultValue(null)]
    public FilterRequestDto? Filter { get; init; }
}

public record FilterRequestDto
{
    [DefaultValue("")]
    public string? Field { get; init; }

    [DefaultValue("")]
    public string? Operator { get; init; }

    [DefaultValue("")]
    public object? Value { get; init; }

    [DefaultValue("")]
    public string? Logic { get; init; }
    public IEnumerable<FilterRequestDto>? Filters { get; init; } = new List<FilterRequestDto>();
}

public record SortRequestDto
{
    [DefaultValue("CreatedAt")]
    public string Field { get; init; } = string.Empty;

    [DefaultValue("desc")]
    public string Dir { get; init; } = "asc";
}

public record PagedBaseRequestDto
{
    [DefaultValue(1)]
    public int PageNumber { get; init; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;

    public string? SortColumn { get; init; } = "CreatedAt";

    [DefaultValue("desc")]
    public string Dir { get; init; } = "asc";
}
