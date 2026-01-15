using Api.Extensions;
using Carter;
using Products.Infrastructure;
using Shared.Api.Extensions;
using Shared.Application;
using Shared.Infrastructure.Extensions;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Assemblies
        var productApplicationAssembly = Products.Application.AssemblyReference.Assembly;
        var productApiAssembly = Products.Api.AssemblyReference.Assembly;

        // Add Application Services
        builder.Services.AddApplication(new[]
        {
            productApplicationAssembly
        });

        // Carter services
        builder.Services.AddCarterWithAssemblies(
            productApiAssembly
        );

        // Masstransit services
        builder.Services.AddMassTransitWithAssemblies(
            builder.Configuration,
            productApiAssembly
        );
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add module
        builder.Services
               .AddProductModule(builder.Configuration);

        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerDocumentation();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapCarter();

        // Use module
        app.UseProductModule();

        app.Run();
    }
}
