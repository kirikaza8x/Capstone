using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Payments.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Configs.Database;

namespace Payment.Infrastructure.Persistence.Contexts
{
    public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentModuleDbContext>
    {
        public PaymentModuleDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = new DatabaseConfig();
            configuration.GetSection("Database").Bind(dbConfig);

            var optionsBuilder = new DbContextOptionsBuilder<PaymentModuleDbContext>();

            optionsBuilder.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
            {

                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Payments.Infrastructure.Constants.SchemaName);

                if (dbConfig.MaxRetryCount > 0)
                    npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                if (dbConfig.CommandTimeout > 0)
                    npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
            })
            .UseSnakeCaseNamingConvention();

            return new PaymentModuleDbContext(optionsBuilder.Options);
        }
    }
}
