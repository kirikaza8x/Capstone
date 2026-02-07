using Shared.Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Shared.Api.Results;

public static class CustomResults
{
    public static IResult Problem(Result result, HttpContext? httpContext = null)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create problem from successful result");
        }

        var extensions = GetErrors(result) ?? new Dictionary<string, object?>();

        if (httpContext != null)
        {
            extensions["traceId"] = httpContext.TraceIdentifier;
        }

        return Microsoft.AspNetCore.Http.Results.Problem(
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            type: GetType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: extensions);
    }

    private static string GetTitle(Error error) => error.Type switch
    {
        ErrorType.Validation => error.Code,
        ErrorType.NotFound => error.Code,
        ErrorType.Conflict => error.Code,
        ErrorType.Unauthorized => error.Code,
        ErrorType.Forbidden => error.Code,
        _ => "Server.Error"
    };

    private static string GetDetail(Error error) => error.Type switch
    {
        ErrorType.Validation => error.Description,
        ErrorType.NotFound => error.Description,
        ErrorType.Conflict => error.Description,
        ErrorType.Unauthorized => error.Description,
        ErrorType.Forbidden => error.Description,
        _ => "An unexpected error occurred"
    };

    private static string GetType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        ErrorType.Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
    };

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
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