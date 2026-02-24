using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Shared.Application.Dtos.Queries;
using System.Text.Json;

public class FilterSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(FilterRequestDto))
        {
            var example = new
            {
                field = "", 
                @operator = "",
                value = "",
                logic = "and",
                filters = new[]
                {
                    // These are clearly marked as examples
                    new { field = "ColumnName", @operator = "eq", value = "ExampleValue" }
                }
            };

            // This ensures Swagger UI shows the nested structure immediately
            schema.Example = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(example));
        }
    }
}