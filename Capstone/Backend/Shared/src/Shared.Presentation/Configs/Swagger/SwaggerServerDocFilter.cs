using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Presentation.Configs.Swagger;

public class ServersDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new OpenApiServer
            {
                Url = "http://localhost:5152",
                Description = "Local ASP.NET Core server"
            },
            new OpenApiServer
            {
                Url = "http://localhost:5151",
                Description = "Local ASP.NET Core server"
            },
            
        };
    }
}
