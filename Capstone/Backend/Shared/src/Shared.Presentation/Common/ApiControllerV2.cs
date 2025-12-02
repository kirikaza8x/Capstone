using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Common.ResponseModel;

namespace Shared.Presentation.Common
{
    [ApiController]
    public abstract class ApiControllerV2 : ControllerBase
    {
        protected readonly IMediator _mediator;

        protected ApiControllerV2(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Handle result with data
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            return result.IsSuccess 
                ? Ok(result) 
                : HandleFailure(result);
        }

        // Handle result without data
        protected IActionResult HandleResult(Result result)
        {
            return result.IsSuccess 
                ? Ok(result) 
                : HandleFailure(result);
        }

        protected IActionResult HandleFailure(Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Cannot handle failure for a successful result");
            }

            return result switch
            {
                // Validation errors -> 400
                IValidationResult validationResult =>
                    BadRequest(CreateProblemDetails(
                        HttpStatusCode.BadRequest,
                        result.Error,
                        validationResult.Errors)),

                // Not found errors -> 404
                _ when result.Error.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase) =>
                    NotFound(CreateProblemDetails(
                        HttpStatusCode.NotFound,
                        result.Error)),

                // Unauthorized errors -> 401
                _ when result.Error.Code.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) =>
                    Unauthorized(CreateProblemDetails(
                        HttpStatusCode.Unauthorized,
                        result.Error)),

                // Forbidden errors -> 403
                _ when result.Error.Code.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) =>
                    StatusCode((int)HttpStatusCode.Forbidden, CreateProblemDetails(
                        HttpStatusCode.Forbidden,
                        result.Error)),

                // Conflict errors -> 409
                _ when result.Error.Code.Contains("Conflict", StringComparison.OrdinalIgnoreCase) ||
                       result.Error.Code.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase) =>
                    Conflict(CreateProblemDetails(
                        HttpStatusCode.Conflict,
                        result.Error)),

                // Default to 400 for other business failures
                _ => BadRequest(CreateProblemDetails(
                        HttpStatusCode.BadRequest,
                        result.Error))
            };
        }

        private static ProblemDetails CreateProblemDetails(
            HttpStatusCode statusCode, 
            Error error, 
            Error[]? errors = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = GetDefaultTitle(statusCode),
                Type = error.Code,
                Detail = error.Description
            };

            if (errors?.Length > 0)
            {
                problemDetails.Extensions["errors"] = errors;
            }

            return problemDetails;
        }

        private static string GetDefaultTitle(HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.Conflict => "Conflict",
            _ => "An error occurred"
        };
    }
}