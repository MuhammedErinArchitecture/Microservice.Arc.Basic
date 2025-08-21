using System.Data;
using BuildingBlocks.Abstractions.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ContentService.Infrastructure.Persistence.Read;

public sealed class ReadDbConnectionFactory : IReadDbConnectionFactory
{
    private readonly string _connStr;
    public ReadDbConnectionFactory(IConfiguration configuration)
    {
        _connStr = configuration.GetConnectionString("ReadDb")
                  ?? throw new InvalidOperationException("ConnectionStrings:ReadDb missing");
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
    {
        var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        return conn;
    }
}
