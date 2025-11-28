using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Common.ResponseModel;
using Shared.Infrastructure.Configs.Security;
using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Common.Exceptions.Handler
{
    public class CustomExceptionHandler(
        ILogger<CustomExceptionHandler> logger,
        IOptions<ErrorHandlingConfigs> options) : IExceptionHandler
    {
        private readonly ErrorHandlingConfigs _config = options.Value;

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError("Unhandled exception: {Message}", exception.Message);

            // Convert exception into your Error record
            var error = Error.FromException(exception);
            var result = Result.Failure(error);

            object response = result;

            // Toggle metadata based on config
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

            // Map exception type to HTTP status code
            context.Response.StatusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(response, cancellationToken: cancellationToken);
            return true;
        }
    }
}
