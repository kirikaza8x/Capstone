
using Microsoft.AspNetCore.Http;
using Shared.Domain.Abstractions;

namespace Shared.Api.Results;

public static class ResultExtensions
{
    public static IResult ToOk(this Result result)
    {
        return result.IsSuccess
            ? Microsoft.AspNetCore.Http.Results.Ok(ApiResult.Success())
            : CustomResults.Problem(result);
    }

    public static IResult ToOk<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Microsoft.AspNetCore.Http.Results.Ok(ApiResult<T>.Success(result.Value))
            : CustomResults.Problem(result);
    }

    public static IResult ToCreated<T>(this Result<T> result, string uri)
    {
        return result.IsSuccess
            ? Microsoft.AspNetCore.Http.Results.Created(uri, ApiResult<T>.Success(result.Value))
            : CustomResults.Problem(result);
    }

    public static IResult ToCreated<T>(this Result<T> result, Func<T, string> uriFactory)
    {
        return result.IsSuccess
            ? Microsoft.AspNetCore.Http.Results.Created(uriFactory(result.Value), ApiResult<T>.Success(result.Value))
            : CustomResults.Problem(result);
    }


    public static IResult ToNoContent(this Result result)
    {
        return result.IsSuccess
            ? Microsoft.AspNetCore.Http.Results.NoContent()
            : CustomResults.Problem(result);
    }
}