using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace Shared.Presentation.Configs.Swagger;

public class DefaultValueSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var properties = context.Type.GetProperties();
        foreach (var property in properties)
        {
            var defaultValueAttr = property
                .GetCustomAttributes(typeof(DefaultValueAttribute), false)
                .FirstOrDefault() as DefaultValueAttribute;

            if (defaultValueAttr != null)
            {
                var jsonName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);

                if (schema.Properties.ContainsKey(jsonName))
                {
                    // Pass the raw value (can be null) into the converter
                    schema.Properties[jsonName].Example = ConvertToOpenApiAny(defaultValueAttr.Value);
                }
            }
        }
    }

    private IOpenApiAny ConvertToOpenApiAny(object? value)
    {
        if (value is null)
            return new OpenApiNull();

        return value switch
        {
            string s => new OpenApiString(s),
            int i => new OpenApiInteger(i),
            long l => new OpenApiLong(l),
            bool b => new OpenApiBoolean(b),
            double d => new OpenApiDouble(d),
            float f => new OpenApiFloat(f),
            Guid g => new OpenApiString(g.ToString()), // represent Guid as string
            Enum e => new OpenApiString(e.ToString()), // enums as string
            _ => new OpenApiString(value.ToString() ?? string.Empty)
        };
    }
}
