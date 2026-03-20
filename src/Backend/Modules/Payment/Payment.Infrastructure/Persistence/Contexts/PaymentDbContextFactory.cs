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
            optionsBuilder.UseNpgsql(dbConfig.ConnectionString);

            return new PaymentModuleDbContext(optionsBuilder.Options);
        }
    }
}