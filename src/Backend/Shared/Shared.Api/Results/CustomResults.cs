using Shared.Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Shared.Api.Results;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create problem from successful result");
        }

        return Microsoft.AspNetCore.Http.Results.Problem(
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            type: GetType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetErrors(result));
    }

    private static string GetTitle(Error error) => error.Type switch
    {
        ErrorType.Validation => error.Code,
        ErrorType.NotFound => error.Code,
        ErrorType.Conflict => error.Code,
        ErrorType.Unauthorized => error.Code,
        _ => "Server.Error"
    };

    private static string GetDetail(Error error) => error.Type switch
    {
        ErrorType.Validation => error.Description,
        ErrorType.NotFound => error.Description,
        ErrorType.Conflict => error.Description,
        ErrorType.Unauthorized => error.Description,
        _ => "An unexpected error occurred"
    };

    private static string GetType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7231#section-6.3.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };

    private static Dictionary<string, object?>? GetErrors(Result result)
    {
        if (result is IValidationResult validationResult)
        {
            return new Dictionary<string, object?>
            {
                { "errors", validationResult.Errors }
            };
        }

        return null;
    }
}
