using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Shared.Presentation.Configs.Swagger;

public static class SwaggerUIConfig
{
    public static void ConfigureSwaggerUI(SwaggerUIOptions c)
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", " API v1");

        // Optional: customize UI
        c.DocumentTitle = " API";
        c.RoutePrefix = string.Empty; 
    }
}
