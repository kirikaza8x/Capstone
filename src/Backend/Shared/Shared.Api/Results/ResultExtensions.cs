
using Microsoft.AspNetCore.Http;
using Shared.Domain.Abstractions;

namespace Shared.Api.Results;

public static class ResultExtensions
{
    public static IResult ToOk(this Result result)
    {
        return result.IsSuccess
            ? TypedResults.Ok(ApiResult.Success())
            : CustomResults.Problem(result);
    }

    public static IResult ToOk<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? TypedResults.Ok(ApiResult<T>.Success(result.Value))
            : CustomResults.Problem(result);
    }

    public static IResult ToOk<T>(this Result<T> result, string successMessage)
    {
        return result.IsSuccess
            ? TypedResults.Ok(ApiResult<T>.Success(result.Value, successMessage))
            : CustomResults.Problem(result);
    }

    public static IResult ToCreated<T>(this Result<T> result, string routeName, Func<T, object> routeValuesFactory)
    {
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                ApiResult<T>.Success(result.Value),
                routeName,
                routeValuesFactory(result.Value))
            : CustomResults.Problem(result);
    }

    public static IResult ToCreated<T>(this Result<T> result, string uri)
    {
        return result.IsSuccess
            ? TypedResults.Created(uri, ApiResult<T>.Success(result.Value))
            : CustomResults.Problem(result);
    }

    public static IResult ToNoContent(this Result result)
    {
        return result.IsSuccess
            ? TypedResults.NoContent()
            : CustomResults.Problem(result);
    }

    public static IResult ToAccepted(this Result result, string? uri = null)
    {
        return result.IsSuccess
            ? TypedResults.Accepted(uri, ApiResult.Success())
            : CustomResults.Problem(result);
    }
}