using Npgsql;
using Shared.Application.Data;
using System.Data.Common;

namespace Shared.Infrastructure.Data;

public class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await dataSource.OpenConnectionAsync();
    }

    public ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
