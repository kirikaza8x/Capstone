using Carter;
using Products.Infrastructure;
using Shared.Api.Extensions;
using Shared.Application.Extensions;
using Shared.Infrastructure.Extensions;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var productAssembly = typeof(ProductModule).Assembly;

        builder.Services
            .AddCarterWithAssemblies(productAssembly);

        builder.Services
            .AddMediatRWithAssemblies(productAssembly);

        builder.Services
            .AddMassTransitWithAssemblies(builder.Configuration, productAssembly);


        builder.Services
            .AddProductModule(builder.Configuration);

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapCarter();
        app.UseProductModule();

        app.Run();
    }
}
