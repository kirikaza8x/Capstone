using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Shared.Presentation.Configs.Swagger;

public static class SwaggerUIConfig
{
    public static void ConfigureSwaggerUI(
    SwaggerUIOptions c,
    string documentName = "v1",
    string serviceName = "API",
    string? routePrefix = null)
    {
        c.SwaggerEndpoint($"/swagger/{documentName}/swagger.json", $"{serviceName} {documentName}");

        c.DocumentTitle = $"{serviceName} Documentation";
        c.RoutePrefix = routePrefix ?? string.Empty;
    }

}