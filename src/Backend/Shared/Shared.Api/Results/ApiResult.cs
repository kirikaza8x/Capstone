
namespace Shared.Api.Results;

public sealed class ApiResult<TData>
{
    public bool IsSuccess { get; init; }
    public TData? Data { get; init; }

    public static ApiResult<TData> Success(TData data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ApiResult<TData> Success() => new()
    {
        IsSuccess = true,
        Data = default
    };
}

public sealed class ApiResult
{
    public bool IsSuccess { get; init; }

    public static ApiResult Success() => new() { IsSuccess = true };
}