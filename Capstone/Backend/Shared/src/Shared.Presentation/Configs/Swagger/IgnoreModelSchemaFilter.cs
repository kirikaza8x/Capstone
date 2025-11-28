using Microsoft.OpenApi.Models;
using Shared.Presentation.Common.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Presentation.Configs.Swagger;
public class IgnoreModelSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // If the model has [SwaggerIgnoreModel], clear it out
        if (context.Type.GetCustomAttributes(typeof(SwaggerIgnoreModelAttribute), true).Any())
        {
            schema.Properties.Clear();
            schema.Type = "object";
            schema.Description = "Hidden from Swagger";
        }
    }
}
