using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Common.ResponseModel;
using Shared.Infrastructure.Configs.Security;
using Shared.Domain.Common.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Shared.Presentation.Handler
{
    /// <summary>
    /// Custom exception handler that intercepts unhandled exceptions and formats a consistent error response.
    /// </summary>
    public class CustomExceptionHandler(
        ILogger<CustomExceptionHandler> logger,
        IOptions<ErrorHandlingConfigs> options) : IExceptionHandler
    {
        private readonly ErrorHandlingConfigs _config = options.Value;

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // Log the exception with full stack trace for diagnostics
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            // Convert exception into domain-specific error model
            var error = Error.FromException(exception);
            var result = Result.Failure(error);

            object response = result;

            // Optionally include metadata for debugging or client diagnostics
            if (_config.IncludeMetadata)
            {
                var metadata = ExceptionMetadataExtractor.Extract(exception);
                response = new
                {
                    result.IsSuccess,
                    result.IsFailure,
                    result.Error,
                    Metadata = metadata
                };
            }

            // Map known exception types to appropriate HTTP status codes
            context.Response.StatusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                // ForbiddenAccessException => StatusCodes.Status403Forbidden,
                // ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            // Set response headers for better client-side error handling
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Error-Type"] = exception.GetType().Name;
            context.Response.Headers["X-Error-Id"] = error.Code ?? Guid.NewGuid().ToString();

            // Write the structured error response
            await context.Response.WriteAsJsonAsync(response, cancellationToken: cancellationToken);
            return true;
        }
    }
}
