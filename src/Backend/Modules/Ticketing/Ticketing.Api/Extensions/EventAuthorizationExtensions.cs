using Events.PublicApi.PublicApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Results;
using Shared.Application.Abstractions.Authentication;
using Shared.Domain.Abstractions;

namespace Ticketing.Api.Extensions;

public static class EventAuthorizationExtensions
{
    public static RouteHandlerBuilder RequireEventPermission(this RouteHandlerBuilder builder, string requiredPermission)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            // get eventId from route
            if (!context.HttpContext.Request.RouteValues.TryGetValue("eventId", out var eventIdObj) ||
                !Guid.TryParse(eventIdObj?.ToString(), out var eventId))
            {
                var error = Error.Failure(
                    "Route.InvalidEventId",
                    "Route must contain a valid eventId.");

                return Result.Failure(error).ToProblem();
            }

            // Get current user id
            var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
            var userId = currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                var error = Error.Unauthorized(
                    "User.Unauthorized",
                    "Current user is not authenticated.");

                return Result.Failure(error).ToProblem();
            }

            // Check permission via IEventMemberPublicApi
            var eventPermissionApi = context.HttpContext.RequestServices.GetRequiredService<IEventMemberPublicApi>();

            var hasPermission = await eventPermissionApi.HasPermissionAsync(eventId, userId, requiredPermission);

            if (!hasPermission)
            {
                var error = Error.Forbidden(
                    "EventPermission.Forbidden",
                    $"User does not have the required '{requiredPermission}' permission for this event.");

                return Result.Failure(error).ToProblem();
            }

            return await next(context);
        });
    }
}
