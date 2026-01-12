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
            var configuration = builder.Configuration;
            var moduleAssemblies = new[]
            {
                UsersApiAssemblyReference.Assembly
            };

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddCarterWithAssemblies(moduleAssemblies);
            builder.Services.AddModulesFromAssemblies(configuration, moduleAssemblies);

            
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapCarter();
            app.MapModules();
            app.MapControllers();
            app.Run();
        }
    }
}
