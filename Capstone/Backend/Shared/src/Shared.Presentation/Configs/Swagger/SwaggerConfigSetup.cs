using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Presentation.Configs.Swagger;

public class SwaggerConfigSetup : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IConfigurationSection _config;

    public SwaggerConfigSetup(IConfiguration configuration)
    {
        _config = configuration.GetSection("SwaggerConfigurations");
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = _config["Title"] ?? "API",
            Version = _config["Version"] ?? "v1",
            Description = _config["Description"] ?? "API documentation for the service"
        });

        options.DocumentFilter<ServersDocumentFilter>();


        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Please enter a valid token",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };
        options.AddSecurityDefinition("Bearer", securityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, Array.Empty<string>() }
        });
        options.SchemaFilter<DefaultValueSchemaFilter>();
        options.SchemaFilter<IgnoreModelSchemaFilter>();
    }
}