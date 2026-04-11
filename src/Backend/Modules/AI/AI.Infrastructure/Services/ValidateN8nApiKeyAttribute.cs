using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marketing.Api.Filters;

/// <summary>
/// Validates X-API-Key header matches configured n8n API key.
/// Use on webhook endpoints only (Carter/minimal APIs).
/// </summary>
public class ValidateN8nApiKeyAttribute : Attribute, IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();
        
        var expectedKey = configuration["N8nIntegration:WebhookApiKey"];
        
        // No key configured = skip validation (dev mode)
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            return await next(context);
        }

        var providedKey = context.HttpContext.Request.Headers["X-API-Key"].FirstOrDefault();
        
        if (providedKey != expectedKey)
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}