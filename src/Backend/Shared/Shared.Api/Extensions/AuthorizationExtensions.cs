using Microsoft.AspNetCore.Builder;

namespace Shared.Api.Extensions;

public static class AuthorizationExtensions
{
    public static RouteHandlerBuilder RequireRoles(this RouteHandlerBuilder builder, params string[] roles)
    {
        return builder.RequireAuthorization(policy =>
        {
            policy.RequireRole(roles);
        });
    }
}
