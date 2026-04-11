using AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Configs.Database;

namespace AIModule.Infrastructure.Persistence.Contexts
{
    public class AIModuleDbContextFactory : IDesignTimeDbContextFactory<AIModuleDbContext>
    {
        public AIModuleDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = new DatabaseConfig();
            configuration.GetSection("Database").Bind(dbConfig);

            var optionsBuilder = new DbContextOptionsBuilder<AIModuleDbContext>();
            optionsBuilder.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", AI.Infrastructure.Constants.SchemaName);

                if (dbConfig.MaxRetryCount > 0)
                    npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                if (dbConfig.CommandTimeout > 0)
                    npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
            })
            .UseSnakeCaseNamingConvention();

            return new AIModuleDbContext(optionsBuilder.Options);
        }
    }
}
