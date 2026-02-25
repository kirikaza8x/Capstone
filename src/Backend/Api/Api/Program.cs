using Ai.Api;
using AI.Application;
using Api.Extensions;
using Api.Middleware;
using Carter;
using Events.Infrastructure;
using Order.Infrastructure;
using Products.Infrastructure;
using Shared.Api.Extensions;
using Shared.Application;
using Shared.Infrastructure.Extensions;
using Users.Api;
using Users.Application;

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
            // product
            Products.Application.AssemblyReference.Assembly,
            Products.Api.AssemblyReference.Assembly,

            // order
            Order.Application.AssemblyReference.Assembly,
            Order.Api.AssemblyReference.Assembly,

            // user
            UsersApplicationAssemblyReference.Assembly,
            UsersApiAssemblyReference.Assembly,

            // event
            Events.Application.AssemblyReference.Assembly,
            Events.Api.AssemblyReference.Assembly,
            
            // AI
            AiApplicationAssemblyReference.Assembly,
            AiApiAssemblyReference.Assembly,
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

        //swagger
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

        // Add storage service
        builder.Services.AddStorageService(Configuration);

        // Add module
        builder.Services
            .AddProductModule(Configuration)
            .AddOrderModule(Configuration)
            .AddUserModule(Configuration)
            .AddEventModule(Configuration)
            .AddAiModule(Configuration)
        ;

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerDocumentation();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseExceptionHandler();

        app.MapCarter();

        app
        .UseOrderModule()
        .UseProductModule()
        .UseUserModule()
        .UseEventModule()
        .UseAiModule()
            ;

        app.Run();
    }
}
