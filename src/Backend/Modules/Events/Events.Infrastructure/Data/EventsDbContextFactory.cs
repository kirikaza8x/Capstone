using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Configs.Database;
// using Events.Infrastructure; // ← Uncomment if Constants is in this namespace

namespace Events.Infrastructure.Data
{
    public class EventsDbContextFactory : IDesignTimeDbContextFactory<EventsDbContext>
    {
        public EventsDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = new DatabaseConfig();
            configuration.GetSection("Database").Bind(dbConfig);

            var optionsBuilder = new DbContextOptionsBuilder<EventsDbContext>();

            optionsBuilder.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
            {

                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);

                if (dbConfig.MaxRetryCount > 0)
                    npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                if (dbConfig.CommandTimeout > 0)
                    npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
            })
            .UseSnakeCaseNamingConvention();

            return new EventsDbContext(optionsBuilder.Options);
        }
    }
}
