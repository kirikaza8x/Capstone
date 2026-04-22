using AI.Api;
using AI.Application;
using Api.Extensions;
using Api.Middleware;
using Carter;
using Events.Infrastructure;
using Notifications.Infrastructure;
using Payments.Api;
using Payments.Application;
using Shared.Api;
using Shared.Api.Results;
using Shared.Application;
using Shared.Infrastructure;
using Ticketing.Infrastructure;
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

            // ticketing
            Ticketing.Application.AssemblyReference.Assembly,
            Ticketing.Api.AssemblyReference.Assembly,
            
            // AI
            AiApplicationAssemblyReference.Assembly,
            ApiAssemblyReference.Assembly,

            // Payment
            PaymentsApplicationAssemblyReference.Assembly,
            PaymentApiAssemblyReference.Assembly,

            // Reports
            Reports.Application.AssemblyReference.Assembly,
            Reports.Api.AssemblyReference.Assembly,

            // Notifications
            Notifications.Application.AssemblyReference.Assembly
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
            .AddNotificationModule(Configuration)
            .AddUserModule(Configuration)
            .AddEventModule(Configuration)
            .AddAiModule(Configuration)
            .AddTicketingModule(Configuration)
            .AddPaymentModule(Configuration)
        ;

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerDocumentation();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();
        app.UseExceptionHandler();

        app.MapGet("/health", () => TypedResults.Ok(ApiResult.Success("Healthy")))
            .AllowAnonymous()
            .WithTags("System")
            .WithName("HealthCheck")
            .WithSummary("Health check")
            .WithDescription("Returns 200 when API is running.");

        app.MapCarter();
        app.UseApi();
        app
        .UseNotificationModule()
        .UseUserModule()
        .UseEventModule()
        .UseAiModule()
        .UseTicketingModule()
        .UsePaymentModule()
        ;
        app.Run();
    }
}
