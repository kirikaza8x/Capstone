using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        dbContext.Database.Migrate();

        return app;
    }
   
}
