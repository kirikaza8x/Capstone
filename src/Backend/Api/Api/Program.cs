using Carter;
using Shared.Api.Extensions;
using Users.Api;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddCarterWithAssemblies(
                typeof(UsersApiAssemblyReference).Assembly
            );
            builder.Services.AddModule<UserModule>(builder.Configuration);
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapCarter();

            app.MapModules();

            app.MapControllers();

            app.Run();
        }
    }
}
