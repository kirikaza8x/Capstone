using Ai.Api;
using AI.Application;
using Api.Extensions;
using Api.Middleware;
using Carter;
using Events.Infrastructure;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Application;
using Shared.Infrastructure;
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
        builder.Services.AddApplication(
            assemblies
            );

        // Add Infrastructure Services
        builder.Services.AddInfrastructure(
            assemblies,
            Configuration
        );

        // Add Api Services
        builder.Services.AddApi(
            assemblies, 
            Configuration
        );

        //swagger
        builder.Services.AddSwaggerDocumentation();


        // Add module
        builder.Services
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
        .UseUserModule()
        .UseEventModule()
        .UseAiModule()
            ;

        app.Run();
    }
}
