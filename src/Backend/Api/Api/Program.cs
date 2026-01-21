using Api.Extensions;
using Carter;
using Products.Infrastructure;
using Shared.Api.Extensions;
using Shared.Application;
using Shared.Infrastructure.Extensions;
using Users.Api;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var Configuration = builder.Configuration;
        // Assemblies array
        var assemblies = new[]
        {
            Products.Application.AssemblyReference.Assembly,
            Products.Api.AssemblyReference.Assembly,
            UsersApiAssemblyReference.Assembly,
        };

        // Add Application Services
        builder.Services.AddApplication(assemblies);

        // Carter services
        builder.Services.AddCarterWithAssemblies(assemblies);

        // Masstransit services
        builder.Services.AddMassTransitWithAssemblies(
            Configuration,
            assemblies
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
        .AddProductModule(Configuration)
        .AddUserModule(Configuration);

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
        //app.UseProductModule();

        app.Run();
    }
}
